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
    public abstract class MultiThreadedFileOperation : FileOperation
    {
        protected ConcurrentQueue<MultiThreadedFileOperationState> FileQueue { get; }
        
        public int MaxThreads { get; set; } = 8;

        protected MultiThreadedFileOperation(
            DirectoryInfo source,
            DirectoryInfo destination, 
            List<ISearchFilter> fileFilters, 
            List<ISearchFilter> directoryFilters,
            FileOperationOptions options) 
            : base(source, destination, fileFilters, directoryFilters, options)
        {
            FileQueue = new ConcurrentQueue<MultiThreadedFileOperationState>();
        }

        #region Protected methods

        protected override void SearchForFiles(CancellationToken cancellationToken = default)
        {
            InitializeThreads();
            base.SearchForFiles(cancellationToken);

            while (FileQueue.Any() && !cancellation.IsCancellationRequested)
                Thread.Sleep(500);
        }

        protected override void OnFileFound(object sender, FileFoundEventArgs args)
        {
            var file = args.Item;
            var target = FileUtils.GetDestinationFile(Source, file, Destination);

            // Block so we don't queue too many files at once.
            while (FileQueue.Count > MaxThreads * 5 && !cancellation.IsCancellationRequested)
                Thread.Sleep(500);

            if (cancellation.IsCancellationRequested)
                return;            

            FileQueue.Enqueue(new MultiThreadedFileOperationState(file, target));
        }

        #endregion

        #region Private methods

        private void InitializeThreads()
        {
            for (var i = 0; i < MaxThreads; i++)
                ThreadPool.QueueUserWorkItem(ThreadProc);
        }

        private void ThreadProc(object state)
        {
            while (!cancellation.IsCancellationRequested)
            {
                MultiThreadedFileOperationState targets;
                while (!FileQueue.TryDequeue(out targets))
                    Thread.Sleep(500);

                var source = targets.SourceFile;
                var target = targets.TargetFile;

                try
                {
                    OnOperationStarted(this, new FileOperationStartedEventArgs(source.FullName));
                    PreOperationHandlers(source, target);
                    ProcessFile(source, target);
                    target.Refresh();
                    PostOperationHandlers(source, target, () => { ProgressHandler(source, target, target.Length, 0); });
                    OnOperationCompleted(this, new FileOperationCompletedEventArgs(target));
                }
                catch (OperationCanceledException)
                {
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
