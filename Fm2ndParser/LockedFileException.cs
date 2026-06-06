using System;
using System.Runtime.Serialization;

namespace Fm2ndParser
{
    [Serializable]
    internal class LockedFileException : Exception
    {
        public LockedFileException()
        {
        }

        public LockedFileException(string message) : base(message)
        {
        }

        public LockedFileException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LockedFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}