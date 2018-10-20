using System;

namespace BananaHomie.ZCopy.FileOperations
{
    internal class FileCopyException : Exception
    {
        public string FileName { get; }

        public FileCopyException(string message, string fileName = default, Exception innerException = default)
            :base(message, innerException)
        {            
            FileName = fileName;
        }
    }
}