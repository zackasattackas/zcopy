using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class FileFoundEventArgs : FileSystemItemFoundEventArgs<FileInfo>
    {
        public FileFoundEventArgs(FileInfo file)
            : base(file)
        {
        }
    }
}