using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class DirectoryFoundEventArgs : FileSystemItemFoundEventArgs<DirectoryInfo>
    {
        public DirectoryFoundEventArgs(DirectoryInfo item) 
            : base(item)
        {
        }
    }
}