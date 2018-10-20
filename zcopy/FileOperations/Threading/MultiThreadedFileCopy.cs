using BananaHomie.ZCopy.FileSystemSearch;
using BananaHomie.ZCopy.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BananaHomie.ZCopy.FileOperations.Threading
{
    public class MultiThreadedFileCopy : MultiThreadedFileOperation
    {        
        #region Fields

        private readonly ConcurrentQueue<MultiThreadedFileOperationState> queue;

        #endregion

        #region Properties

        public CopyOptions Options { get; set; }

        #endregion

        #region Ctor

        public MultiThreadedFileCopy(
            DirectoryInfo source, 
            DirectoryInfo destination, 
            List<ISearchFilter> fileFilters, 
            List<ISearchFilter> directoryFilters) 
            : base(source, destination, fileFilters, directoryFilters)
        {
            queue = new ConcurrentQueue<MultiThreadedFileOperationState>();
        }

        #endregion

        #region Public methods

        public override void Invoke(CancellationToken cancellationToken = default)
        {
            base.cancellation = cancellationToken;

            InitializeThreads();                        
            SearchForAndCopyFiles();

            while (queue.Any() && !cancellation.IsCancellationRequested)
                Thread.Sleep(500);
        }

        #endregion

        #region Internal methods

        internal override string GetOptionsString()
        {
            return Options.ToString();
        }

        #endregion

        #region Private methods

        private void InitializeThreads()
        {
            for (var i = 0; i < MaxThreads; i++)
                ThreadPool.QueueUserWorkItem(CopyThreadProc);
        }

        private void SearchForAndCopyFiles()
        {
            var searchOption = Options.HasFlag(CopyOptions.Recurse) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var search = new FileSystemSearch.FileSystemSearch(Source, FileFilters, DirectoryFilters, searchOption);
            search.FileFound += SearchOnFileFound;
            search.Error += SearchOnError;

            search.Search(cancellation);
        }

        #region File system search event handlers

        private void SearchOnFileFound(object sender, FileFoundEventArgs e)
        {
            var file = e.Item;
            
            if (cancellation.IsCancellationRequested)
                return;

            var target = FileUtils.GetDestinationFile(Source, file, Destination);            

            // Block so we don't queue too many files at once.
            while (queue.Count > MaxThreads * 5 && !cancellation.IsCancellationRequested)
                Thread.Sleep(500);

            queue.Enqueue(new MultiThreadedFileOperationState(file, target));
        }

        private void SearchOnError(object sender, FileSystemSearchErrorEventArgs e)
        {            
            OnError(sender, new FileOperationErrorEventArgs(e.Exception));
        }

        #endregion

        private void CopyThreadProc(object state)
        {
            while (!cancellation.IsCancellationRequested)
            {
                MultiThreadedFileOperationState targets;
                while (!queue.TryDequeue(out targets))
                    Thread.Sleep(500);
                
                var source = targets.SourceFile;
                var target = targets.TargetFile;

                try
                {
                    OnOperationStarted(this, new FileOperationStartedEventArgs(source.FullName));

                    PreOperationHandlers(source, target);

                    FileUtils.CopyFile(source, target, BufferSize, WhatToCopy, ProgressHandler, cancellation);

                    if (cancellation.IsCancellationRequested)
                        break;

                    PostOperationHandlers(source, target, () => { ProgressHandler(source, target, target.Length, 0); });

                    OnCompleted(this, new FileOperationCompletedEventArgs(target));

                    lock (SynchronizingObject)
                    {
                        Statistics.BytesTransferred += target.Length;
                        Statistics.TotalFiles++;
                    }
                }
                catch (Exception e)
                {
                    OnError(this, new FileOperationErrorEventArgs(e));
                }
            }
        }

        #endregion
    }
}