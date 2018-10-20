using System;
using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class FileSystemSearchErrorEventArgs : EventArgs
    {
        public FileSystemSearchErrorEventArgs(FileSystemInfo item, Exception exception)
        {
            Item = item;
            Exception = exception;
        }

        public FileSystemInfo Item { get; }
        public Exception Exception { get; }
    }
}