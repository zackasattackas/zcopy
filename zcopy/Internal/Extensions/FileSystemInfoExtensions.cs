using BananaHomie.ZCopy.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BananaHomie.ZCopy.Internal.Extensions
{
    internal static class FileSystemInfoExtensions
    {
        public static bool MatchesAll(this FileSystemInfo item, IEnumerable<ISearchFilter> filters)
        {
            return filters.All(f => f.IsMatch(item));
        }

        public static bool IsDirectory(this FileSystemInfo item)
        {
            return item.Attributes.HasFlag(FileAttributes.Directory);
        }

        //public static bool IsCompressed(this FileSystemInfo item)
        //{
        //    return item.Attributes.HasFlag(FileAttributes.Compressed);
        //}

        //public static bool IsSystem(this FileSystemInfo item)
        //{
        //    return item.Attributes.HasFlag(FileAttributes.System);
        //}

        //public static bool IsReparsePoint(this FileSystemInfo item)
        //{
        //    return item.Attributes.HasFlag(FileAttributes.ReparsePoint);
        //}

        //public static bool IsHidden(this FileSystemInfo item)
        //{
        //    return item.Attributes.HasFlag(FileAttributes.Hidden);
        //}

        //public static bool IsEncrypted(this FileSystemInfo item)
        //{
        //    return item.Attributes.HasFlag(FileAttributes.Encrypted);
        //}

        public static bool IsShadowProtectImageFile(this FileInfo file)
        {
            return file.Extension == ".spi" || file.Extension == ".spf";
        }

        public static string ReadAllText(this FileInfo file)
        {
            using (var freader = file.OpenText())
                return freader.ReadToEnd();
        }
    }
}
