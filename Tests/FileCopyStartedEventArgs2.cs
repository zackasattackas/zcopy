using System;
using System.IO;

namespace Tests
{
    public class FileCopyStartedEventArgs2 : EventArgs
    {
        public FileInfo Source { get; set; }
        public FileInfo Destination { get; set; }

        public FileCopyStartedEventArgs2(FileInfo source, FileInfo destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}