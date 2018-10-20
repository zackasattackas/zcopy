﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using BananaHomie.ZCopy.FileSystemSearch;

namespace BananaHomie.ZCopy.FileOperations.Threading
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

        private void SearchOnError(object sender, FileSystemSearchErrorEventArgs e)
        {
            switch (e.Item)
            {
                case FileInfo _:
                    Statistics.SkippedFiles++;
                    break;
                case DirectoryInfo _:
                    Statistics.SkippedDirectories++;
                    break;
            }

            OnError(sender, new FileOperationErrorEventArgs(e.Exception));
        }
    }
}