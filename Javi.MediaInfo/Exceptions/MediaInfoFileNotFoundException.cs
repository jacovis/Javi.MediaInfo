// -----------------------------------------------------------------------
// <copyright file="MediaInfoFileNotFoundException.cs">
// All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Javi.MediaInfo
{
    using System;

    public class MediaInfoFileNotFoundException : Exception
    {
        public MediaInfoFileNotFoundException()
        {
        }

        public MediaInfoFileNotFoundException(string message)
            : base(message)
        {
        }

        public MediaInfoFileNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
