// -----------------------------------------------------------------------
// <copyright file="AudioInfo.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Javi.MediaInfo
{
    using System;

    public class AudioInfo
    {
        public int Id { get; set; }
        public string Format { get; set; }
        public string FormatInfo { get; set; }
        public string FormatCommercial { get; set; }
        public string CodecId { get; set; }
        public TimeSpan Duration { get; set; }
        public int BitRate { get; set; }
        public string BitRateString { get; set; }
        public string BitRateModeAbbreviation { get; set; }
        public string BitRateMode { get; set; }
        public int Channels { get; set; }
        public string ChannelPositions { get; set; }
        public int SamplingRate { get; set; }
        public double FrameRate { get; set; }
        public int FrameCount { get; set; }
        public string CompressionMode { get; set; }
        public int StreamSize { get; set; }
        public string LanguageCode { get; set; }
        public string Language { get; set; }
        public bool Default { get; set; }
        public bool Forced { get; set; }
    }
}
