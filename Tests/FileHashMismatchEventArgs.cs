using System;
using System.IO;

namespace Tests
{
    public class FileHashMismatchEventArgs : EventArgs
    {
        public FileHashMismatchEventArgs(FileInfo file, int offset, byte expectedValue, byte actualValue)
        {
            File = file;
            Offset = offset;
            ExpectedValue = expectedValue;
            ActualValue = actualValue;
        }

        public FileInfo File { get;  }
        public int Offset { get;  }
        public byte ExpectedValue { get;  }
        public byte ActualValue { get; }
    }
}