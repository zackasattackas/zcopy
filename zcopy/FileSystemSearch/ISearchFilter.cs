using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public interface ISearchFilter
    {
        bool IsMatch(FileSystemInfo item);
    }
}