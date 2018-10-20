using System.Collections.Generic;
using System.IO;
using BananaHomie.ZCopy.FileSystemSearch;

namespace BananaHomie.ZCopy.FileOperations.Threading
{
    public abstract class MultiThreadedFileOperation : FileOperation
    { 
        public int MaxThreads { get; set; } = 8;

        protected MultiThreadedFileOperation(
            DirectoryInfo source,
            DirectoryInfo destination, 
            List<ISearchFilter> fileFilters, 
            List<ISearchFilter> directoryFilters) 
            : base(source, destination, fileFilters, directoryFilters)
        {
        }
    }
}
