using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BananaHomie.ZCopy.IO.Threading
{
    public class MultiThreadedFileMove : MultiThreadedFileOperation
    {
        public MultiThreadedFileMove(
            DirectoryInfo source, 
            DirectoryInfo destination, 
            List<ISearchFilter> fileFilters, 
            List<ISearchFilter> directoryFilters) 
            : base(source, destination, fileFilters, directoryFilters)
        {
        }

        public override void Invoke(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        internal override string GetOptionsString()
        {
            return string.Empty;
        }
    }
}