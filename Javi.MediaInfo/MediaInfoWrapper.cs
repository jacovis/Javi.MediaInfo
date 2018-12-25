// -----------------------------------------------------------------------
// <copyright file="MediaInfoLib.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Javi.MediaInfo
{
    using System;
    using System.Runtime.InteropServices;

    #region enumerators
    // enumerators copied from MediaInfo\MediaInfo_DLL_18.05_Windows_x64_WithoutInstaller\Developers\Project\MSCS2010\Example\MediaInfoDLL.cs
    public enum StreamKind
    {
        General,
        Video,
        Audio,
        Text,
        Other,
        Image,
        Menu,
    }

    public enum InfoKind
    {
        Name,
        Text,
        Measure,
        Options,
        NameText,
        MeasureText,
        Info,
        HowTo
    }

    public enum InfoOptions
    {
        ShowInInform,
        Support,
        ShowInSupported,
        TypeOfValue
    }

    public enum InfoFileOptions
    {
        FileOption_Nothing = 0x00,
        FileOption_NoRecursive = 0x01,
        FileOption_CloseAll = 0x02,
        FileOption_Max = 0x04
    };

    public enum Status
    {
        None = 0x00,
        Accepted = 0x01,
        Filled = 0x02,
        Updated = 0x04,
        Finalized = 0x08,
    }
    #endregion

    /// <summary>
    /// Wrapper for MediaInfo library dll from https://mediaarea.net/en/MediaInfo/Download/Windows
    /// This DLL is provided in 32 and 64 bit versions. 
    /// The code in this class uses dynamic DLL loading to load the correct version and call functions in it. 
    /// Any application using this assembly should still be able to be build using AnyCPU. 
    /// Code in this class is based on class MediaInfoDLL.cs in the mentioned download in source file Developers\Project\MSCS2010\Example\MediaInfoDLL.cs.
    /// See code in the example in the download in folder Developers\Project\MSCS2010\Example\HowToUse_Dll.cs for samples of usage of the media info lib functions.
    /// See ActiveX.cls in the MediaInfo source in download libmediainfo_18.05_AllInclusive.zip for descriptions of the various functions in MediaInfo.dll.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class MediaInfoWrapper : IDisposable
    {
        #region fields
        private readonly IntPtr handleDLL = IntPtr.Zero;
        private readonly IntPtr handleMediaInfo;
        #endregion

        #region properties
        public string MediaInfoDLLVersion { get; }
        public string[] InfoParametersCSV { get; }
        #endregion

        #region dynamically loaded DLL functions
        // Create a new MediaInfo interface and return a handle to it
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MediaInfo_New();
        private readonly MediaInfo_New mediaInfo_New;

        // Delete a MediaInfo interface
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MediaInfo_Delete(IntPtr Handle);
        private readonly MediaInfo_Delete mediaInfo_Delete;

        // Configure or get information about MediaInfoLib
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);
        private readonly MediaInfo_Option mediaInfo_Option;

        // Open a file and collect information about it (technical information and tags)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);
        private readonly MediaInfo_Open mediaInfo_Open;

        // Close a file opened before with Open() (without saving)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MediaInfo_Close(IntPtr Handle);
        private readonly MediaInfo_Close mediaInfo_Close;

        // Get details about a file in one string; see HowToUse_Dll.cs in the download for examples of use
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MediaInfo_Inform(IntPtr Handle, IntPtr Reserved);
        private readonly MediaInfo_Inform mediaInfo_Inform;

        // Get a piece of information about a file (parameter is a string)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MediaInfo_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
        private readonly MediaInfo_Get mediaInfo_Get;

        // Get a piece of information about a file (parameter is a string)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MediaInfo_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
        private readonly MediaInfo_GetI mediaInfo_GetI;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaInfoWrapper" /> class.
        /// </summary>
        /// <param name="mediaInfoDLLFullName">Full path and filename of the media information DLL.</param>
        /// <exception cref="MediaInfoException">
        /// </exception>
        public MediaInfoWrapper(string mediaInfoDLLFullName)
        {
            try
            {
                handleDLL = PInvoke.LoadLibrary(mediaInfoDLLFullName);
            }
            catch (Exception ex)
            {
                throw new MediaInfoLoadException($"Exception on LoadLibrary({mediaInfoDLLFullName}): ", ex);
            }
            if (handleDLL == IntPtr.Zero)
            {
                throw new MediaInfoLoadException($"Unable to dynamically load {mediaInfoDLLFullName}");
            }
            else
            {
                // get pointers to functions used in the loaded dll
                IntPtr address = PInvoke.GetProcAddress(handleDLL, "MediaInfo_New");
                if (address == IntPtr.Zero) { throw new MediaInfoException("GetProcAddress failed for MediaInfo_New"); }
                this.mediaInfo_New = (MediaInfo_New)Marshal.GetDelegateForFunctionPointer(address, typeof(MediaInfo_New));

                address = PInvoke.GetProcAddress(handleDLL, "MediaInfo_Delete");
                if (address == IntPtr.Zero) { throw new MediaInfoException("GetProcAddress failed for MediaInfo_Delete"); }
                this.mediaInfo_Delete = (MediaInfo_Delete)Marshal.GetDelegateForFunctionPointer(address, typeof(MediaInfo_Delete));

                address = PInvoke.GetProcAddress(handleDLL, "MediaInfo_Option");
                if (address == IntPtr.Zero) { throw new MediaInfoException("GetProcAddress failed for MediaInfo_Option"); }
                this.mediaInfo_Option = (MediaInfo_Option)Marshal.GetDelegateForFunctionPointer(address, typeof(MediaInfo_Option));

                address = PInvoke.GetProcAddress(handleDLL, "MediaInfo_Open");
                if (address == IntPtr.Zero) { throw new MediaInfoException("GetProcAddress failed for MediaInfo_Open"); }
                this.mediaInfo_Open = (MediaInfo_Open)Marshal.GetDelegateForFunctionPointer(address, typeof(MediaInfo_Open));

                address = PInvoke.GetProcAddress(handleDLL, "MediaInfo_Close");
                if (address == IntPtr.Zero) { throw new MediaInfoException("GetProcAddress failed for MediaInfo_Close"); }
                this.mediaInfo_Close = (MediaInfo_Close)Marshal.GetDelegateForFunctionPointer(address, typeof(MediaInfo_Close));

                address = PInvoke.GetProcAddress(handleDLL, "MediaInfo_Inform");
                if (address == IntPtr.Zero) { throw new MediaInfoException("GetProcAddress failed for MediaInfo_Inform"); }
                this.mediaInfo_Inform = (MediaInfo_Inform)Marshal.GetDelegateForFunctionPointer(address, typeof(MediaInfo_Inform));

                address = PInvoke.GetProcAddress(handleDLL, "MediaInfo_Get");
                if (address == IntPtr.Zero) { throw new MediaInfoException("GetProcAddress failed for MediaInfo_Get"); }
                this.mediaInfo_Get = (MediaInfo_Get)Marshal.GetDelegateForFunctionPointer(address, typeof(MediaInfo_Get));

                address = PInvoke.GetProcAddress(handleDLL, "MediaInfo_GetI");
                if (address == IntPtr.Zero) { throw new MediaInfoException("GetProcAddress failed for MediaInfo_GetI"); }
                this.mediaInfo_GetI = (MediaInfo_GetI)Marshal.GetDelegateForFunctionPointer(address, typeof(MediaInfo_GetI));

                // create new mediainfo interface
                try
                {
                    this.handleMediaInfo = this.mediaInfo_New();
                }
                catch
                {
                    this.handleMediaInfo = IntPtr.Zero;
                }

                // read the version from the DLL using a mediainfo interface definition
                this.MediaInfoDLLVersion = this.Option("Info_Version");

                // Get all parameters usable for the dll function Get. 
                this.InfoParametersCSV = this.Option("Info_Parameters_CSV").Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Open a file and collect information about it (technical information and tags).
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>0 if failed to open file.</returns>
        public int Open(String filename)
        {
            if (handleMediaInfo == IntPtr.Zero) { return 0; }

            return (int)this.mediaInfo_Open(handleMediaInfo, filename);
        }

        /// <summary>
        /// Close a file opened before with Open() (without saving).
        /// </summary>
        public void Close()
        {
            if (handleMediaInfo == IntPtr.Zero) { return; }

            this.mediaInfo_Close(handleMediaInfo);
        }

        /// <summary>
        /// Get details about a file in one string; see HowToUse_Dll.cs in the download for examples of use.
        /// </summary>
        public String Inform()
        {
            if (handleMediaInfo == IntPtr.Zero)
            {
                return "Unable to load MediaInfo library";
            }

            return Marshal.PtrToStringUni(mediaInfo_Inform(handleMediaInfo, IntPtr.Zero));
        }

        /// <summary>
        /// Get a piece of information about a file (parameter is a string)
        /// </summary>
        public String Get(StreamKind StreamKind, int StreamNumber, String Parameter, InfoKind KindOfInfo, InfoKind KindOfSearch)
        {
            if (handleMediaInfo == IntPtr.Zero)
            {
                return "Unable to load MediaInfo library";
            }

            return Marshal.PtrToStringUni(this.mediaInfo_Get(handleMediaInfo, (IntPtr)StreamKind, (IntPtr)StreamNumber, Parameter, (IntPtr)KindOfInfo, (IntPtr)KindOfSearch));
        }

        /// <summary>
        /// Get a piece of information about a file (parameter is a string)
        /// </summary>
        public String Get(StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo)
        {
            if (handleMediaInfo == IntPtr.Zero)
            {
                return "Unable to load MediaInfo library";
            }

            return Marshal.PtrToStringUni(this.mediaInfo_GetI(handleMediaInfo, (IntPtr)StreamKind, (IntPtr)StreamNumber, (IntPtr)Parameter, (IntPtr)KindOfInfo));
        }

        public String Get(StreamKind StreamKind, int StreamNumber, String Parameter, InfoKind KindOfInfo)
        {
            return Get(StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name);
        }

        public String Get(StreamKind StreamKind, int StreamNumber, String Parameter)
        {
            return Get(StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name);
        }

        public String Get(StreamKind StreamKind, int StreamNumber, int Parameter)
        {
            return Get(StreamKind, StreamNumber, Parameter, InfoKind.Text);
        }

        /// <summary>
        /// Configure or get information about MediaInfoLib.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="value">The value is an optional parameter for the specified option.</param>
        /// <returns></returns>
        public String Option(string option, string value = "")
        {
            if (this.handleMediaInfo == IntPtr.Zero)
            {
                return "Unable to load MediaInfo library";
            }

            return Marshal.PtrToStringUni(this.mediaInfo_Option(this.handleMediaInfo, option, value));
        }

        #region IDisposable
        private bool isdisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isdisposed)
            {
                // Clean-up Unmanaged
                if (handleDLL != IntPtr.Zero)
                {
                    if (this.handleMediaInfo != IntPtr.Zero)
                    {
                        mediaInfo_Delete(handleMediaInfo);
                    }
                    if (PInvoke.FreeLibrary(handleDLL) == false)
                    {
                        throw new MediaInfoException("FreeLibrary failed");
                    }
                }

                this.isdisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MediaInfoWrapper"/> class.
        /// </summary>
        ~MediaInfoWrapper()
        {
            Dispose(false);
        }
        #endregion
    }
}
