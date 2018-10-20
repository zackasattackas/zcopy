using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.FileOperations
{
    internal abstract class FileEqualityComparer : IEqualityComparer<FileInfo>
    {
        public abstract bool Equals(FileInfo x, FileInfo y);

        public int GetHashCode(FileInfo obj)
        {
            return obj.Length.GetHashCode() ^ obj.LastWriteTime.Ticks.GetHashCode();
        }
    }
}
