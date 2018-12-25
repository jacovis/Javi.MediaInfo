// -----------------------------------------------------------------------
// <copyright file="TextInfo.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Javi.MediaInfo
{
    using System;

    public class TextInfo
    {
        public int Id { get; set; }
        public string Format { get; set; }
        public string Codec { get; set; }
        public string CodecInfo { get; set; }
        public TimeSpan Duration { get; set; }
        public int Count { get; set; }
        public int StreamSize { get; set; }
        public string LanguageCode { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public bool Default { get; set; }
        public bool Forced { get; set; }
    }
}
