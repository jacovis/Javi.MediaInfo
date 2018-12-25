// -----------------------------------------------------------------------
// <copyright file="MediaInfoLoadException.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Javi.MediaInfo
{
    using System;

    public class MediaInfoLoadException : Exception
    {
        public MediaInfoLoadException()
        {
        }

        public MediaInfoLoadException(string message)
            : base(message)
        {
        }

        public MediaInfoLoadException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
