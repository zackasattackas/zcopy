using System;

namespace BananaHomie.ZCopy.FileOperations
{
    public class FileOperationStartedEventArgs : EventArgs
    {
        public FileOperationStartedEventArgs(string fullPath)
        {
            FullPath = fullPath;
        }

        public string FullPath { get; }
    }
}