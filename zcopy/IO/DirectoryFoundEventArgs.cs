using System.IO;

namespace BananaHomie.ZCopy.IO
{
    public class DirectoryFoundEventArgs : FileSystemItemFoundEventArgs<DirectoryInfo>
    {
        public DirectoryFoundEventArgs(DirectoryInfo item) 
            : base(item)
        {
        }
    }
}