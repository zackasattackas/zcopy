using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BananaHomie.ZCopy.FileSystemSearch;
using BananaHomie.ZCopy.Internal;

namespace BananaHomie.ZCopy.FileOperations.Threading
{
    public class MultiThreadedFileCopy : MultiThreadedFileOperation
    {        
        #region Fields

        [Obsolete]
        private readonly Dictionary<Guid, MultiThreadedFileOperationState> threadTable;

        private ConcurrentQueue<MultiThreadedFileOperationState> queue;

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
            //threadTable = new Dictionary<Guid, MultiThreadedFileOperationState>();
            queue = new ConcurrentQueue<MultiThreadedFileOperationState>();
        }

        #endregion

        #region Public methods

        public override void Invoke(CancellationToken cancellationToken = default)
        {
            base.cancellation = cancellationToken;

            InitializeThreadTable();                        
            SearchForAndCopyFiles();
            DisposeThreadTable();
        }

        #endregion

        #region Internal methods

        internal override string GetOptionsString()
        {
            return Options.ToString();
        }

        #endregion

        #region Private methods

        private void InitializeThreadTable()
        {
            for (var i = 0; i < MaxThreads; i++)
            {
                //var threadState = new MultiThreadedFileOperationState();
                
                //threadTable.Add(threadState.Guid, threadState);
                ThreadPool.QueueUserWorkItem(CopyThreadProc);
            }
        }

        private void DisposeThreadTable()
        {
            while (queue.Any())
                Thread.Sleep(500);
            //foreach (var item in threadTable)
            //    item.Value.Dispose();
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

            var target = Utilities.GetDestinationFile(Source, file, Destination);            

            // Block so we don't queue too many files at once.
            while (queue.Count > MaxThreads * 5 && !cancellation.IsCancellationRequested)
                Thread.Sleep(500);

            queue.Enqueue(new MultiThreadedFileOperationState(file, target));

            //if (cancellation.IsCancellationRequested)
            //    return;

            //Guid threadId;

            //lock (SynchronizingObject)
            //{
            //    threadId = threadTable.First(t => t.Value.IsReady && t.Value.Queue.Count < 5).Key;
            //    //threadTable[threadId].SourceFile = file;
            //    //threadTable[threadId].TargetFile = target;
            //    threadTable[threadId].IsCompleted = false;
            //    threadTable[threadId].Queue.Enqueue((file, target));
            //    threadTable[threadId].WaitHandle.Set();
            //}

            //// Wait for thread to start copying file
            //while (IsThreadReady(threadId))
            //    Thread.Sleep(500);
        }

        private void SearchOnError(object sender, FileSystemSearchErrorEventArgs e)
        {
            // I should do something here
        }

        #endregion

        private void CopyThreadProc(object state)
        {
            //var threadId = (Guid) state;

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

                    Utilities.CopyFile(source, target, BufferSize, WhatToCopy, ProgressHandler, cancellation);

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
                //finally
                //{
                //    lock (SynchronizingObject)
                //    {
                //        threadTable[threadId].IsReady = true;
                //        threadTable[threadId].IsCompleted = true;
                //    }
                //}
            }

            //lock (SynchronizingObject)
            //{
            //    threadTable[threadId].IsReady = true;
            //    threadTable[threadId].IsCompleted = true;
            //}
        }

        //private AutoResetEvent GetThreadWaitHandle(Guid threadId)
        //{
        //    lock (SynchronizingObject) return threadTable[threadId].WaitHandle;
        //}

        //private bool AllThreadsBusy()
        //{
        //    lock (SynchronizingObject)
        //        return threadTable.All(t => t.Value.Queue.Count >= 5);
        //}

        //private bool IsThreadReady(Guid threadId)
        //{
        //    lock (SynchronizingObject) return threadTable[threadId].IsReady && !threadTable[threadId].IsCompleted;
        //}

        #endregion
    }
}