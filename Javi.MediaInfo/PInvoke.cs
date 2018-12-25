// -----------------------------------------------------------------------
// <copyright file="PInvoke.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Javi.MediaInfo
{
    /// <summary>
    /// Platform invoke helper class.
    /// </summary>
    public static class PInvoke
    {
        #region LoadLibrary, GetProcAddress, FreeLibrary
        // see: https://blogs.msdn.microsoft.com/jonathanswift/2006/10/03/dynamically-calling-an-unmanaged-dll-from-net-c/
        // Dynamically calling an unmanaged dll from .NET (C#)
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        #endregion
    }
}