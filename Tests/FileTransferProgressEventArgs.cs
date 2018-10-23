using System.IO;

namespace Tests
{
    public class FileTransferProgressEventArgs : FileCopyProgressEventArgs
    {
        public FileTransferProgressEventArgs(FileInfo source, FileInfo destination, FileTransferProgressData progress)
            : base(source, destination)
        {
            Progress = progress;
        }


        public FileTransferProgressData Progress { get;}
    }
}