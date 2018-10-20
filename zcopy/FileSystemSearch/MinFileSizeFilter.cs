using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class MinFileSizeFilter : ISearchFilter
    {
        private readonly double minLength;

        public MinFileSizeFilter(double minLength)
        {
            this.minLength = minLength;
        }

        public bool IsMatch(FileSystemInfo item)
        {
            return item is FileInfo file && file.Length >= minLength;
        }
    }
}