using System;
using System.IO;
using BananaHomie.ZCopy.IO;

namespace BananaHomie.ZCopy.Commands
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
