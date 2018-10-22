using System;
using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class DelegatingSearchFilter : ISearchFilter
    {
        private readonly Func<FileSystemInfo, bool> predicate;

        public DelegatingSearchFilter(Func<FileSystemInfo, bool> func)
        {
            predicate = func;
        }

        public bool IsMatch(FileSystemInfo item)
        {
            return predicate(item);
        }

        public static implicit operator DelegatingSearchFilter(Func<FileSystemInfo, bool> func)
        {
            return new DelegatingSearchFilter(func);
        }
    }
}
