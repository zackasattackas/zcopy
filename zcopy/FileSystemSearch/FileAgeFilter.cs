using System;
using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class FileAgeFilter : ISearchFilter
    {
        public DateTime Value { get; set; }
        public bool IsUtc { get; set; }
        public bool IsMaximum { get; set; }

        public FileAgeFilter(string value, bool isUtc,bool isMaximum)
        {
            Value = DateTime.Parse(value);
            IsUtc = isUtc;
            IsMaximum = isMaximum;
        }

        public bool IsMatch(FileSystemInfo item)
        {
            var creationTime = IsUtc ? item.CreationTimeUtc : item.CreationTime;

            return creationTime >= Value && IsMaximum;
        }
    }
}
