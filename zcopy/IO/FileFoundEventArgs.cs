using System.IO;

namespace BananaHomie.ZCopy.IO
{
    public class FileFoundEventArgs : FileSystemItemFoundEventArgs<FileInfo>
    {
        public FileFoundEventArgs(FileInfo file)
            : base(file)
        {
        }
    }
}