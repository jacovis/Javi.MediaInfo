// -----------------------------------------------------------------------
// <copyright file="MediaInfoException.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Javi.MediaInfo
{
    using System;

    public class MediaInfoException : Exception
    {
        public MediaInfoException()
        {
        }

        public MediaInfoException(string message)
            : base(message)
        {
        }

        public MediaInfoException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
