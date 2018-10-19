using System.IO;

namespace BananaHomie.ZCopy.IO
{
    public interface ISearchFilter
    {
        bool IsMatch(FileSystemInfo item);
    }
}