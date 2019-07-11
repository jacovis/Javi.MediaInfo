// -----------------------------------------------------------------------
// <copyright file="MediaInfo.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Javi.MediaInfo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    public class MediaInfo
    {
        private readonly string MediaInfoDLLFullName;
        public List<string> Information { get; private set; }
        public GeneralInfo General { get; private set; }
        public List<VideoInfo> Video { get; private set; }
        public List<AudioInfo> Audio { get; private set; }
        public List<TextInfo> Text { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaInfo" /> class.
        /// </summary>
        public MediaInfo(string mediaInfoDLLFullName)
        {
            this.MediaInfoDLLFullName = mediaInfoDLLFullName;
            if (!File.Exists(this.MediaInfoDLLFullName))
            {
                throw new MediaInfoFileNotFoundException(this.MediaInfoDLLFullName);
            }

            this.Information = new List<string>();
            this.General = new GeneralInfo();
            this.Video = new List<VideoInfo>();
            this.Audio = new List<AudioInfo>();
            this.Text = new List<TextInfo>();
        }

        /// <summary>Reads the media information from the specified file.</summary>
        /// <param name="mediaInfoDLLFullName">Full filename of the mediainfo.DLL.</param>
        /// <param name="fileName">The media file full filename.</param>
        public void ReadMediaInformation(string fileName)
        {
            using (MediaInfoWrapper mediaInfo = new MediaInfoWrapper(this.MediaInfoDLLFullName))
            {
                mediaInfo.Open(fileName);

                try
                {
                    this.ReadInformation(mediaInfo);
                    this.ReadGeneral(mediaInfo);
                    this.ReadVideo(mediaInfo);
                    this.ReadAudio(mediaInfo);
                    this.ReadText(mediaInfo);
                }
                finally
                {
                    mediaInfo.Close();
                }
            }
        }

        /// <summary>
        /// Get allparameter values and dump to logfile.
        /// call:
        /// var mi = new MediaInfo(fullFileName);
        /// string s = mi.AllParameterValues(fullFileName);
        /// </summary>
        /// <param name="fileName">The filename.</param>
        /// <returns></returns>
        public string AllParameterValues(string mediaInfoDLLFullName, string fileName)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(fileName);

            using (MediaInfoWrapper mediaInfo = new MediaInfoWrapper(mediaInfoDLLFullName))
            {
                mediaInfo.Open(fileName);
                try
                {
                    StreamKind streamKind = StreamKind.General;
                    int streamCount = 0;
                    foreach (string parameter in mediaInfo.InfoParametersCSV)
                    {
                        string[] parameters = parameter.Split(';');
                        if (parameters.Length == 1)
                        {
                            sb.AppendLine(parameters[0]);
                            Enum.TryParse(parameters[0], out streamKind);
                            if (Int32.TryParse(mediaInfo.Get(streamKind, 0, "StreamCount"), out streamCount))
                            {
                                sb.AppendLine("streamCount=" + streamCount.ToString());
                            }
                        }
                        else if (parameters.Length > 1)
                        {
                            ////parameters[0]
                            ////"Status"
                            ////parameters[1]
                            ////"bit field (0=IsAccepted, 1=IsFilled, 2=IsUpdated, 3=IsFinished)"
                            for (int i = 0; i < streamCount; i++)
                            {
                                string value = mediaInfo.Get(streamKind, i, parameters[0]);
                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    sb.AppendLine(String.Format("{0}: {1}={2} ({3})", streamKind, parameters[0], value, parameters[1]));
                                }
                            }
                        }
                    }
                }
                finally
                {
                    mediaInfo.Close();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Reads the information from the file opened using the mediaInfo object.
        /// Property Information will contain the information equal to the information shown in the MediaInfo application, using menu View -> Text
        /// </summary>
        /// <param name="mediaInfo">The media information library.</param>
        private void ReadInformation(MediaInfoWrapper mediaInfo)
        {
            foreach (string item in mediaInfo.Inform().Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                this.Information.Add(item);
            }
        }

        /// <summary>
        /// Reads the general information from the file opened using the mediainfo object.
        /// </summary>
        /// <param name="mediaInfo">The media information library.</param>
        private void ReadGeneral(MediaInfoWrapper mediaInfo)
        {
            this.General.Format = mediaInfo.Get(StreamKind.General, 0, "Format");
            this.General.FormatVersion = mediaInfo.Get(StreamKind.General, 0, "Format_Version");
            if (Double.TryParse(mediaInfo.Get(StreamKind.General, 0, "Duration"), NumberStyles.Any, CultureInfo.InvariantCulture, out double duration))
            {
                this.General.Duration = TimeSpan.FromMilliseconds(duration);
            }
            Int32.TryParse(mediaInfo.Get(StreamKind.General, 0, "OverallBitRate"), out int overallBitRate);
            this.General.OverallBitRate = overallBitRate;
            this.General.OverallBitRateAsString = mediaInfo.Get(StreamKind.General, 0, "OverallBitRate/String");
            // excepted format from the mediainfo dll: 'UTC 2018-06-18 01:16:56'
            string ed = mediaInfo.Get(StreamKind.General, 0, "Encoded_Date");
            if (!string.IsNullOrEmpty(ed) && ed.Length == 23 && ed.Substring(0, 4) == "UTC ")
            {
                try
                {
                    ed = ed.Substring(4);
                    ed = ed.Replace(' ', 'T');
                    this.General.EncodedDate = DateTime.Parse(ed);
                }
                catch (FormatException)
                {
                    // mediainfo returned invalid format for us to parse
                    this.General.EncodedDate = default(DateTime);
                }
            }
            this.General.WritingApplication = mediaInfo.Get(StreamKind.General, 0, "Encoded_Application/String");
            this.General.WritingLibrary = mediaInfo.Get(StreamKind.General, 0, "Encoded_Library/String");
            if (Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "StreamCount"), out int streamCount))
            {
                General.VideoStreams = streamCount;
            }
            if (Int32.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "StreamCount"), out streamCount))
            {
                General.AudioStreams = streamCount;
            }
            if (Int32.TryParse(mediaInfo.Get(StreamKind.Text, 0, "StreamCount"), out streamCount))
            {
                General.TextStreams = streamCount;
            }

            // Additions for audio files
            this.General.AlbumPerformer = mediaInfo.Get(StreamKind.General, 0, "Album/Performer");
            this.General.Performer = mediaInfo.Get(StreamKind.General, 0, "Performer");
            this.General.Album = mediaInfo.Get(StreamKind.General, 0, "Album");
            this.General.Title = mediaInfo.Get(StreamKind.General, 0, "Track");
            if (string.IsNullOrEmpty(this.General.Title)) { this.General.Title = mediaInfo.Get(StreamKind.General, 0, "TrackName"); }
            this.General.Genre = mediaInfo.Get(StreamKind.General, 0, "Genre");
            if (Int32.TryParse(mediaInfo.Get(StreamKind.General, 0, "Track/Position"), out int trackPosition))
            {
                this.General.TrackPosition = trackPosition;
            }
            if (Int32.TryParse(mediaInfo.Get(StreamKind.General, 0, "Track/Position_Total"), out int totalTracks))
            {
                this.General.TotalTracks = totalTracks;
            }
            this.General.RecordedDate = mediaInfo.Get(StreamKind.General, 0, "Recorded_Date");
            this.General.Publisher = mediaInfo.Get(StreamKind.General, 0, "Publisher");
        }

        /// <summary>
        /// Reads the video information from the file opened using the mediainfo object.
        /// </summary>
        /// <param name="mediaInfo">The media information library.</param>
        private void ReadVideo(MediaInfoWrapper mediaInfo)
        {
            if (Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "StreamCount"), out int videoStreamCount))
            {
                for (int i = 0; i < videoStreamCount; i++)
                {
                    VideoInfo vi = new VideoInfo();

                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, i, "ID"), out int ID);
                    vi.Id = ID;
                    vi.Format = mediaInfo.Get(StreamKind.Video, i, "Format");
                    vi.FormatInfo = mediaInfo.Get(StreamKind.Video, i, "Format/Info");
                    vi.FormatProfile = mediaInfo.Get(StreamKind.Video, i, "Format_Profile");
                    vi.FormatSettings = mediaInfo.Get(StreamKind.Video, i, "Format_Settings");
                    vi.CodecId = mediaInfo.Get(StreamKind.Video, i, "CodecID");
                    vi.CodecIdString = mediaInfo.Get(StreamKind.Video, i, "CodecID/String");
                    vi.CodecIdInfo = mediaInfo.Get(StreamKind.Video, i, "CodecID/Info");
                    vi.CodecIdHint = mediaInfo.Get(StreamKind.Video, i, "CodecID/Hint");
                    vi.CodecIdURL = mediaInfo.Get(StreamKind.Video, i, "CodecID/Url");
                    vi.CodecIdDescription = mediaInfo.Get(StreamKind.Video, i, "CodecID_Description");
                    if (Double.TryParse(mediaInfo.Get(StreamKind.Video, i, "Duration"), NumberStyles.Any, CultureInfo.InvariantCulture, out double duration))
                    {
                        vi.Duration = TimeSpan.FromMilliseconds(duration);
                    }
                    vi.BitRateModeAbbreviation = mediaInfo.Get(StreamKind.Video, i, "BitRate_Mode");
                    vi.BitRateMode = mediaInfo.Get(StreamKind.Video, i, "BitRate_Mode/String");
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, i, "BitRate"), out int bitrate);
                    vi.BitRate = bitrate;
                    vi.BitRateString = mediaInfo.Get(StreamKind.Video, i, "BitRate/String");
                    if (string.IsNullOrWhiteSpace(vi.BitRateString))
                    {
                        Int32.TryParse(mediaInfo.Get(StreamKind.Video, i, "BitRate_Nominal"), out bitrate);
                        vi.BitRate = bitrate;
                        vi.BitRateString = mediaInfo.Get(StreamKind.Video, i, "BitRate_Nominal/String");
                    }
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, i, "Width"), out int width);
                    vi.Width = width;
                    vi.WidthString = mediaInfo.Get(StreamKind.Video, i, "Width/String");
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, i, "Height"), out int height);
                    vi.Height = height;
                    vi.HeightString = mediaInfo.Get(StreamKind.Video, i, "Height/String");
                    Double.TryParse(mediaInfo.Get(StreamKind.Video, i, "DisplayAspectRatio"), NumberStyles.Any, CultureInfo.InvariantCulture, out double displayAspectRatio);
                    vi.DisplayAspectRatio = displayAspectRatio;
                    vi.DisplayAspectRatioString = mediaInfo.Get(StreamKind.Video, i, "DisplayAspectRatio/String");
                    Double.TryParse(mediaInfo.Get(StreamKind.Video, i, "FrameRate"), NumberStyles.Any, CultureInfo.InvariantCulture, out double frameRate);
                    if (frameRate == 0) { Double.TryParse(mediaInfo.Get(StreamKind.Video, i, "FrameRate_Original"), NumberStyles.Any, CultureInfo.InvariantCulture, out frameRate); }
                    vi.FrameRate = frameRate;
                    vi.FrameRateString = mediaInfo.Get(StreamKind.Video, i, "FrameRate/String");
                    if (string.IsNullOrWhiteSpace(vi.FrameRateString)) { vi.FrameRateString = mediaInfo.Get(StreamKind.Video, i, "FrameRate_Original/String"); }
                    vi.FrameRateModeAbbreviation = mediaInfo.Get(StreamKind.Video, i, "FrameRate_Mode");
                    if (string.IsNullOrWhiteSpace(vi.FrameRateModeAbbreviation)) { vi.FrameRateModeAbbreviation = mediaInfo.Get(StreamKind.Video, i, "FrameRate_Mode_Original"); }
                    vi.FrameRateMode = mediaInfo.Get(StreamKind.Video, i, "FrameRate_Mode/String");
                    if (string.IsNullOrWhiteSpace(vi.FrameRateMode)) { vi.FrameRateMode = mediaInfo.Get(StreamKind.Video, i, "FrameRate_Mode_Original/String"); }
                    vi.ColorSpace = mediaInfo.Get(StreamKind.Video, i, "ColorSpace");
                    vi.ScanType = mediaInfo.Get(StreamKind.Video, i, "ScanType");
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, i, "StreamSize"), out int streamSize);
                    vi.StreamSize = streamSize;
                    vi.Default = (mediaInfo.Get(StreamKind.Video, i, "Default").ToUpper() == "YES" ? true : false);

                    this.Video.Add(vi);
                }
            }
        }

        /// <summary>
        /// Reads the audio information from the file opened using the mediainfolobject.
        /// </summary>
        /// <param name="mediaInfo">The media information library.</param>
        private void ReadAudio(MediaInfoWrapper mediaInfo)
        {
            if (Int32.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "StreamCount"), out int audioStreamCount))
            {
                for (int i = 0; i < audioStreamCount; i++)
                {
                    AudioInfo ai = new AudioInfo();

                    Int32.TryParse(mediaInfo.Get(StreamKind.Audio, i, "ID"), out int ID);
                    ai.Id = ID;
                    ai.Format = mediaInfo.Get(StreamKind.Audio, i, "Format");
                    ai.FormatInfo = mediaInfo.Get(StreamKind.Audio, i, "Format/Info");
                    ai.FormatVersion = mediaInfo.Get(StreamKind.Audio, i, "Format_Version");
                    ai.FormatProfile = mediaInfo.Get(StreamKind.Audio, i, "Format_Profile");
                    ai.FormatLevel = mediaInfo.Get(StreamKind.Audio, i, "Format_Level");
                    ai.FormatCompression = mediaInfo.Get(StreamKind.Audio, i, "Format_Compression");
                    ai.FormatSettings = mediaInfo.Get(StreamKind.Audio, i, "Format_Settings");
                    ai.FormatSettingsMode = mediaInfo.Get(StreamKind.Audio, i, "Format_Settings_Mode");
                    ai.FormatSettingsModeExtension = mediaInfo.Get(StreamKind.Audio, i, "Format_Settings_ModeExtension");
                    ai.FormatCommercial = mediaInfo.Get(StreamKind.Audio, i, "Format_Commercial_IfAny");
                    if (string.IsNullOrEmpty(ai.FormatCommercial)) { ai.FormatCommercial = ai.FormatInfo; }
                    if (string.IsNullOrEmpty(ai.FormatCommercial)) { ai.FormatCommercial = mediaInfo.Get(StreamKind.Audio, i, "CodecID/Hint"); }
                    ai.CodecId = mediaInfo.Get(StreamKind.Audio, i, "CodecID");
                    ai.WritingLibrary = mediaInfo.Get(StreamKind.Audio, i, "Encoded_Library");
                    ai.EncodingSettings = mediaInfo.Get(StreamKind.Audio, i, "Encoded_Library_Settings");
                    if (string.IsNullOrEmpty(ai.WritingLibrary)) { ai.WritingLibrary = mediaInfo.Get(StreamKind.Audio, i, "Encoded_Library/String"); }
                    if (Double.TryParse(mediaInfo.Get(StreamKind.Audio, i, "Duration"), NumberStyles.Any, CultureInfo.InvariantCulture, out double duration))
                    {
                        ai.Duration = TimeSpan.FromMilliseconds(duration);
                    }
                    Int32.TryParse(mediaInfo.Get(StreamKind.Audio, i, "BitRate"), out int bitrate);
                    ai.BitRate = bitrate;
                    ai.BitRateString = mediaInfo.Get(StreamKind.Audio, i, "BitRate/String");
                    ai.BitRateModeAbbreviation = mediaInfo.Get(StreamKind.Audio, i, "BitRate_Mode");
                    ai.BitRateMode = mediaInfo.Get(StreamKind.Audio, i, "BitRate_Mode/String");
                    Int32.TryParse(mediaInfo.Get(StreamKind.Audio, i, "Channel(s)"), out int channels);
                    ai.Channels = channels;
                    ai.ChannelPositions = mediaInfo.Get(StreamKind.Audio, i, "ChannelPositions");
                    Int32.TryParse(mediaInfo.Get(StreamKind.Audio, i, "SamplingRate"), out int samplingRate);
                    ai.SamplingRate = samplingRate;
                    Double.TryParse(mediaInfo.Get(StreamKind.Audio, i, "FrameRate"), NumberStyles.Any, CultureInfo.InvariantCulture, out double frameRate);
                    ai.FrameRate = frameRate;
                    Int32.TryParse(mediaInfo.Get(StreamKind.Audio, i, "FrameCount"), out int frameCount);
                    ai.FrameCount = frameCount;
                    ai.CompressionMode = mediaInfo.Get(StreamKind.Audio, i, "Compression_Mode");
                    Int32.TryParse(mediaInfo.Get(StreamKind.Audio, i, "StreamSize"), out int streamSize);
                    ai.StreamSize = streamSize;
                    ai.LanguageCode = mediaInfo.Get(StreamKind.Audio, i, "Language");
                    ai.Language = mediaInfo.Get(StreamKind.Audio, i, "Language/String");
                    ai.Default = (mediaInfo.Get(StreamKind.Audio, i, "Default").ToUpper() == "YES" ? true : false);
                    ai.Forced = (mediaInfo.Get(StreamKind.Audio, i, "Forced").ToUpper() == "YES" ? true : false);

                    this.Audio.Add(ai);
                }
            }
        }

        /// <summary>
        /// Reads the subtitle information from the file opened using the mediainfo object.
        /// </summary>
        /// <param name="mediaInfo">The media information library.</param>
        private void ReadText(MediaInfoWrapper mediaInfo)
        {
            if (Int32.TryParse(mediaInfo.Get(StreamKind.Text, 0, "StreamCount"), out int textStreamCount))
            {
                for (int i = 0; i < textStreamCount; i++)
                {
                    TextInfo ti = new TextInfo();
                    Int32.TryParse(mediaInfo.Get(StreamKind.Text, i, "ID"), out int ID);
                    ti.Id = ID;
                    ti.Format = mediaInfo.Get(StreamKind.Text, i, "Format");
                    ti.Codec = mediaInfo.Get(StreamKind.Text, i, "Codec");
                    ti.CodecInfo = mediaInfo.Get(StreamKind.Text, i, "Codec/Info");
                    if (Double.TryParse(mediaInfo.Get(StreamKind.Text, i, "Duration"), NumberStyles.Any, CultureInfo.InvariantCulture, out double duration))
                    {
                        ti.Duration = TimeSpan.FromMilliseconds(duration);
                    }
                    else
                    {
                        string fsd = mediaInfo.Get(StreamKind.Text, i, "FromStats_Duration");
                        if (fsd.Length >= 12)
                        {
                            TimeSpan.TryParse(fsd.Substring(0, 12), out TimeSpan ts);
                            ti.Duration = ts;
                        }
                    }
                    if (!Int32.TryParse(mediaInfo.Get(StreamKind.Text, i, "ElementCount"), out int count) || count == 0)
                    {
                        Int32.TryParse(mediaInfo.Get(StreamKind.Text, i, "FromStats_FrameCount"), out count);
                    }
                    ti.Count = count;
                    Int32.TryParse(mediaInfo.Get(StreamKind.Text, i, "StreamSize"), out int streamSize);
                    ti.StreamSize = streamSize;
                    ti.LanguageCode = mediaInfo.Get(StreamKind.Text, i, "Language");
                    ti.Language = mediaInfo.Get(StreamKind.Text, i, "Language/String");
                    ti.Title = mediaInfo.Get(StreamKind.Text, i, "Title");
                    ti.Default = (mediaInfo.Get(StreamKind.Text, i, "Default").ToUpper() == "YES" ? true : false);
                    ti.Forced = (mediaInfo.Get(StreamKind.Text, i, "Forced").ToUpper() == "YES" ? true : false);

                    this.Text.Add(ti);
                }
            }
        }
    }
}