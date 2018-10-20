using System.IO;

namespace BananaHomie.ZCopy.FileOperations
{
    internal class BasicFileComparer : FileEqualityComparer
    {
        public override bool Equals(FileInfo x, FileInfo y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            if (ReferenceEquals(x, y))
                return true;
            if (x.Length != y.Length)
                return false;
            if (x.Name != y.Name)
                return false;

            return x.Extension == y.Extension;
        }
    }
}