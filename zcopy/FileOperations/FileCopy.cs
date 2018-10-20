using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BananaHomie.ZCopy.FileOperations.Threading;
using BananaHomie.ZCopy.FileSystemSearch;
using BananaHomie.ZCopy.Internal;

namespace BananaHomie.ZCopy.FileOperations
{
    public class FileCopy : FileOperation
    {
        #region Fields

        private int skippedDirectories;
        private int skippedFiles;

        #endregion

        #region Properties

        public CopyOptions Options { get; }

        #endregion

        #region Ctor

        public FileCopy(
            DirectoryInfo source,
            DirectoryInfo destination,
            List<ISearchFilter> fileFilters,
            List<ISearchFilter> directoryFilters,
            CopyOptions options)
            : base(source, destination, fileFilters, directoryFilters)
        {
            Options = options;

            if (VerifyNeeded())
                Handlers.Add(new MD5Verification());
        }

        #endregion

        #region Public methods

        public override void Invoke(CancellationToken cancellationToken = default)
        {
            base.cancellation = cancellationToken;
            var searchOption = Options.HasFlag(CopyOptions.Recurse) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var search = new FileSystemSearch.FileSystemSearch(Source, FileFilters, DirectoryFilters, searchOption);
            search.FileFound += SearchOnFileFound;
            search.Error += SearchOnError;
               
            search.Search(cancellationToken);
        }

        public MultiThreadedFileCopy MakeMultiThreaded(int threadCount)
        {
            return new MultiThreadedFileCopy(Source, Destination, FileFilters, DirectoryFilters)
                {BufferSize = BufferSize, Credentials = Credentials, WhatToCopy = WhatToCopy, MaxThreads = threadCount, Options = Options};
        }

        #endregion

        #region Private methods

        private void SearchOnError(object sender, FileSystemSearchErrorEventArgs e)
        {
            switch (e.Item)
            {
                case FileInfo _:
                    skippedFiles++;
                    break;
                case DirectoryInfo _:
                    skippedDirectories++;
                    break;
            }
        }

        private void SearchOnFileFound(object sender, FileFoundEventArgs args)
        {           
            var file = args.Item;
            OnOperationStarted(this, new FileOperationStartedEventArgs(file.FullName));

            if (cancellation.IsCancellationRequested)
                return;

            var target = Utilities.GetDestinationFile(Source, file, Destination);

            try
            {
                PreOperationHandlers(file, target);

                Utilities.CopyFile(file, target, BufferSize, WhatToCopy, ProgressHandler, cancellation);

                if (cancellation.IsCancellationRequested)
                    return;

                target.Refresh();

                PostOperationHandlers(file, target, () => { ProgressHandler(file, target, target.Length, 0); });
                ProgressHandler(file, target, target.Length, 0);

                OnCompleted(this, new FileOperationCompletedEventArgs(target));
            }
            catch (Exception e)
            {
                OnError(this, new FileOperationErrorEventArgs(e));
            }

            Statistics.TotalFiles++;
            Statistics.BytesTransferred += file.Length;
        }

        internal override string GetOptionsString()
        {
            return Options.ToString();
        }

        private bool VerifyNeeded()
        {
            return Options.HasFlag(CopyOptions.VerifyMD5);
        }

        #endregion
    }
}