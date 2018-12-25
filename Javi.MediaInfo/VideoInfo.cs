// -----------------------------------------------------------------------
// <copyright file="VideoInfo.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Javi.MediaInfo
{
    using System;

    public class VideoInfo
    {
        public int Id { get; set; }
        public string Format { get; set; }
        public string FormatInfo { get; set; }
        public string FormatProfile { get; set; }
        public string FormatSettings { get; set; }
        public string CodecId { get; set; }
        public string CodecIdString { get; set; }
        public string CodecIdInfo { get; set; }
        public string CodecIdHint { get; set; }
        public string CodecIdURL { get; set; }
        public string CodecIdDescription { get; set; }
        public TimeSpan Duration { get; set; }
        public string BitRateModeAbbreviation { get; set; }
        public string BitRateMode { get; set; }
        public int BitRate { get; set; }
        public string BitRateString { get; set; }
        public int Width { get; set; }
        public string WidthString { get; set; }
        public int Height { get; set; }
        public string HeightString { get; set; }
        public double DisplayAspectRatio { get; set; }
        public string DisplayAspectRatioString { get; set; }
        public double FrameRate { get; set; }
        public string FrameRateString { get; set; }
        public string FrameRateModeAbbreviation { get; set; }
        public string FrameRateMode { get; set; }
        public string ColorSpace { get; set; }
        public string ScanType { get; set; }
        public int StreamSize { get; set; }
        public bool Default { get; set; }
        public bool Forced { get; set; }
    }
}
