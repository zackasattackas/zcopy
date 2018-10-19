using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.IO.Threading
{
    public abstract class MultiThreadedFileOperation : FileOperation
    {
        protected object SynchronizingObject { get; }
        public int MaxThreads { get; set; } = 8;

        protected MultiThreadedFileOperation(
            DirectoryInfo source,
            DirectoryInfo destination, 
            List<ISearchFilter> fileFilters, 
            List<ISearchFilter> directoryFilters) 
            : base(source, destination, fileFilters, directoryFilters)
        {
            SynchronizingObject = new object();
        }
    }
}
