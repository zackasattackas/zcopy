using System.IO;

namespace BananaHomie.ZCopy.FileOperations
{
    internal class FileComparerWithAttributes : BasicFileComparer
    {
        public override bool Equals(FileInfo x, FileInfo y)
        {
            if (!base.Equals(x, y))
                return false;

            return x?.Attributes == y?.Attributes;
        }
    }
}