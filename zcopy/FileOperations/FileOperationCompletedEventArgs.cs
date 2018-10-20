using System;
using System.IO;

namespace BananaHomie.ZCopy.FileOperations
{
    public class FileOperationCompletedEventArgs : EventArgs
    {
        public FileOperationCompletedEventArgs(FileInfo file)
        {
            File = file;
        }

        public FileInfo File { get; }

    }
}