using System;
using System.IO;

namespace Tests
{
    public abstract class FileCopyProgressEventArgs : EventArgs
    {
        protected FileCopyProgressEventArgs(FileInfo source, FileInfo destination)
        {
            Source = source;
            Destination = destination;
        }

        public FileInfo Source { get; }
        public FileInfo Destination { get; }
    }
}