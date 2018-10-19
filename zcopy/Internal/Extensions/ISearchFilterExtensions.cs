using BananaHomie.ZCopy.Commands;
using BananaHomie.ZCopy.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.Internal.Extensions
{
    internal static class SearchFilterExtensions
    {
        public static void Add(this IList<ISearchFilter> e, Func<FileSystemInfo, bool> filter)
        {
            e.Add(new DelegatingSearchFilter(filter));
        }
    }
}
