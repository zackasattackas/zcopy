using System.IO;

namespace BananaHomie.ZCopy.FileOperations
{
    internal class FileComparerWithTimeStamps : FileComparerWithAttributes
    {
        public override bool Equals(FileInfo x, FileInfo y)
        {
            if (!base.Equals(x, y))
                return false;
            if (x.Attributes != y.Attributes)
                return false;
            if (x.CreationTime != y.CreationTime)
                return false;

            return x.LastWriteTime == y.LastWriteTime;
        }
    }
}