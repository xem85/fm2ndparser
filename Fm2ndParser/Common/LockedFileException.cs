using System;
using System.Runtime.Serialization;

namespace Fm2ndParser.Common
{
    [Serializable]
    class LockedFileException : Exception
    {
        public LockedFileException(string filename)
            : base($"The file {filename} is locked, and can't be parsed.")
        {
        }
    }
}