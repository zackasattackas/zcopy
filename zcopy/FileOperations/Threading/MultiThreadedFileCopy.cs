using BananaHomie.ZCopy.FileSystemSearch;
using System;
using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.FileOperations.Threading
{
    public class MultiThreadedFileCopy : MultiThreadedFileOperation
    {        
        #region Ctor

        public MultiThreadedFileCopy(
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
                TryCopyFile(source, destination);
            }
            catch (Exception e)
            {
                OnError(this, new FileOperationErrorEventArgs(e));
            }
        }

        #endregion
    }
}