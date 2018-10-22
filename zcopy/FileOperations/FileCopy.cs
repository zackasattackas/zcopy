using BananaHomie.ZCopy.FileOperations.Threading;
using BananaHomie.ZCopy.FileSystemSearch;
using System;
using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.FileOperations
{
    public class FileCopy : FileOperation
    {
        #region Ctor

        public FileCopy(
            DirectoryInfo source,
            DirectoryInfo destination,
            List<ISearchFilter> fileFilters,
            List<ISearchFilter> directoryFilters,
            FileOperationOptions options)
            : base(source, destination, fileFilters, directoryFilters, options)
        {
            if (options.HasFlag(FileOperationOptions.VerifyMD5))
                Handlers.Add(new MD5Verification());
        }

        #endregion

        #region Public methods

        public MultiThreadedFileCopy MakeMultiThreaded(int threadCount)
        {
            return new MultiThreadedFileCopy(Source, Destination, FileFilters, DirectoryFilters, Options)
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