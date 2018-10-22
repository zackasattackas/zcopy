using BananaHomie.ZCopy.FileSystemSearch;
using System;
using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.FileOperations.Threading
{
    public class MultiThreadedFileMove : MultiThreadedFileOperation
    {
        #region Ctor

        public MultiThreadedFileMove(
            DirectoryInfo source, 
            DirectoryInfo destination, 
            List<ISearchFilter> fileFilters, 
            List<ISearchFilter> directoryFilters,
            FileOperationOptions options) 
            : base(source, destination, fileFilters, directoryFilters, options)
        {
        }

        #endregion

        #region Protected methods

        protected override void ProcessFile(FileInfo source, FileInfo destination)
        {
            try
            {
                TryMoveFile(source, destination);
            }
            catch (Exception e)
            {
                OnError(this, new FileOperationErrorEventArgs(e));
            }
        }

        #endregion
    }
}