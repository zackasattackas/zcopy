using System;

namespace BananaHomie.ZCopy.FileOperations
{
    public class FileOperationErrorEventArgs : EventArgs
    {
        public FileOperationErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; set; }

    }
}