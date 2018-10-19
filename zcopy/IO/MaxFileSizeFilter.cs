using System.IO;

namespace BananaHomie.ZCopy.IO
{

    public class MaxFileSizeFilter : ISearchFilter
    {
        private readonly double maxLength;

        public MaxFileSizeFilter(double maxLength)
        {
            this.maxLength = maxLength;
        }

        public bool IsMatch(FileSystemInfo item)
        {
            return item is FileInfo file && file.Length <= maxLength;
        }
    }
}