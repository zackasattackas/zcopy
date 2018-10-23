using BananaHomie.ZCopy.FileOperations.Threading;
using BananaHomie.ZCopy.FileSystemSearch;
using System;
using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.FileOperations
{
    public class FileMoveOperation : FileOperation
    {
        #region Ctor

        public FileMoveOperation(
            DirectoryInfo source,
            DirectoryInfo destination,
            List<ISearchFilter> fileFilters,
            List<ISearchFilter> directoryFilters,
            FileOperationOptions options)
            : base(source, destination, fileFilters, directoryFilters, options)
        {
        }

        #endregion

        #region Public methods

        public MultiThreadedFileMove MakeMultiThreaded(int threadCount)
        {
            return new MultiThreadedFileMove(Source, Destination, FileFilters, DirectoryFilters, Options)
            {
                BufferSize = BufferSize,
                Credentials = Credentials,
                WhatToCopy = WhatToCopy,
                MaxThreads = threadCount,
                Options = Options
            };
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