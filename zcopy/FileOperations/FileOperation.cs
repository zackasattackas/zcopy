using BananaHomie.ZCopy.FileSystemSearch;
using BananaHomie.ZCopy.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;

namespace BananaHomie.ZCopy.FileOperations
{
    public abstract class FileOperation : IDisposable
    {
        #region Fields

        protected WindowsImpersonationContext impersonationContext;
        protected CancellationToken cancellation;
        private NetworkCredential credentials;

        #endregion

        #region Properties

        protected object SynchronizingObject { get; }
        public FileOperationOptions Options { get; set; }
        public List<IFileOperationHandler> Handlers { get; }
        public DirectoryInfo Source { get; set; }
        public DirectoryInfo Destination { get; set; }
        public List<ISearchFilter> FileFilters { get; set; }
        public List<ISearchFilter> DirectoryFilters { get; set; }
        public int BufferSize { get; set; }
        public FileCopyStats Statistics { get; protected set; }
        //public TimeSpan InterPacketGap { get; set; }
        //public TimeSpan StartTimeOfDay { get; set; }
        //public TimeSpan StopTimeOfDay { get; set; }
        public WhatToCopy WhatToCopy { get; set; } = WhatToCopy.Data;
        public int RetryCount { get; set; }
        public TimeSpan RetryInterval { get; set; }
        public NetworkCredential Credentials
        {
            protected get => credentials;
            set
            {
                impersonationContext?.Undo();
                impersonationContext?.Dispose();
                impersonationContext = Impersonation.ImpersonateUser(value);
                credentials = value;
            }
        }

        #endregion

        #region Events

        public event EventHandler<FileOperationStartedEventArgs> OperationStarted;
        public event EventHandler<FileOperationProgressEventArgs> ChunkFinished;
        public event EventHandler<FileOperationCompletedEventArgs> OperationCompleted;
        public event EventHandler<FileOperationErrorEventArgs> Error;
        public event EventHandler<FileOperationRetryStartedEventArgs> RetryStarted; 

        #endregion

        #region Ctor

        protected FileOperation(
            DirectoryInfo source, 
            DirectoryInfo destination, 
            List<ISearchFilter> fileFilters, 
            List<ISearchFilter> directoryFilters, 
            FileOperationOptions options)
        {
            SynchronizingObject = new object();
            Source = source;
            Destination = destination;
            FileFilters = fileFilters;
            DirectoryFilters = directoryFilters;
            Statistics = new FileCopyStats();
            Handlers = new List<IFileOperationHandler>();
            Options = options;
        }

        #endregion

        #region Public methods

        public void Start(CancellationToken cancellationToken = default)
        {
            SearchForFiles(cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected methods        

        protected virtual void SearchForFiles(CancellationToken cancellationToken = default)
        {
            cancellation = cancellationToken;
            var searchOption = Options.HasFlag(FileOperationOptions.Recurse) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var search = new FileSystemSearch.FileSystemSearch(Source, FileFilters, DirectoryFilters, searchOption);
            search.FileFound += OnFileFound;
            search.Error += OnSearchError;

            search.Search(cancellationToken);
        }

        protected abstract void ProcessFile(FileInfo source, FileInfo destination);

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                impersonationContext?.Undo();
                impersonationContext?.Dispose();
            }
        }

        protected virtual void OnFileFound(object sender, FileFoundEventArgs args)
        {
            var file = args.Item;
            var target = FileUtils.GetDestinationFile(Source, file, Destination);

            OnOperationStarted(this, new FileOperationStartedEventArgs(file.FullName));
            
            if (cancellation.IsCancellationRequested)
                return;

            PreOperationHandlers(file, target);
            ProcessFile(file, target);
            PostOperationHandlers(file, target, () => { ProgressHandler(file, target, target.Length, 0); });
            ProgressHandler(file, target, target.Length, 0);

            OnOperationCompleted(this, new FileOperationCompletedEventArgs(target));
        }

        protected void OnSearchError(object sender, FileSystemSearchErrorEventArgs args)
        {
            lock (SynchronizingObject)
                switch (args.Item)
                {
                    case FileInfo _:
                        Statistics.SkippedFiles++;
                        break;
                    case DirectoryInfo _:
                        Statistics.SkippedDirectories++;
                        break;
                }

            OnError(sender, new FileOperationErrorEventArgs(args.Exception));
        }

        protected void OnOperationStarted(object sender, FileOperationStartedEventArgs args)
        {
            OperationStarted?.Invoke(sender, args);
        }

        protected void OnChunkFinished(object sender, FileOperationProgressEventArgs args)
        {
            ChunkFinished?.Invoke(sender, args);
        }

        protected void OnOperationCompleted(object sender, FileOperationCompletedEventArgs args)
        {
            OperationCompleted?.Invoke(sender, args);
        }

        protected void OnError(object sender, FileOperationErrorEventArgs args)
        {
            Error?.Invoke(sender, args);
        }

        protected void OnRetryStarted(object sender, FileOperationRetryStartedEventArgs args)
        {
            RetryStarted?.Invoke(sender, args);
        }

        protected void PreOperationHandlers(FileInfo source, FileInfo target, Action waitIterationCallback = default)
        {
            if (!Handlers.Any())
                return;

            var callbackCancellation = waitIterationCallback == default ? null : new CancellationTokenSource();

            if (waitIterationCallback != null)
                ThreadPool.QueueUserWorkItem(state =>
                {
                    var cancellationToken = (CancellationToken) state;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        waitIterationCallback.Invoke();
                        Thread.Sleep(ZCopyConfiguration.RefreshInterval);
                    }
                }, callbackCancellation.Token);
            
            Handlers.ForEach(h => h.OnPreProcessing(source, target));            
            WaitHandle.WaitAll(Handlers.Select(h => h.WaitHandle).ToArray());
            callbackCancellation?.Cancel();
            callbackCancellation?.Dispose();
        }

        protected void PostOperationHandlers(FileInfo source, FileInfo target, Action waitIterationCallback = default)
        {
            if (!Handlers.Any())
                return;

            var callbackCancellation = waitIterationCallback == default ? null : new CancellationTokenSource();

            if (waitIterationCallback != null)
                ThreadPool.QueueUserWorkItem(state =>
                {
                    var cancellationToken = (CancellationToken) state;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        waitIterationCallback.Invoke();
                        Thread.Sleep(ZCopyConfiguration.RefreshInterval);
                    }
                }, callbackCancellation.Token);

            Handlers.ForEach(h => h.OnPostProcessing(source, target));
            WaitHandle.WaitAll(Handlers.Select(h => h.WaitHandle).ToArray());
            callbackCancellation?.Cancel();
            callbackCancellation?.Dispose();
        }

        protected void TryCopyFile(FileInfo source, FileInfo target)
        {
            var tries = 0;
            while (true)
            {
                try
                {
                    FileUtils.CopyFile(source, target, BufferSize, WhatToCopy, ProgressHandler, cancellation);

                    lock (SynchronizingObject)
                    {
                        Statistics.TotalFiles++;
                        Statistics.BytesTransferred += target.Length;
                    }
 
                    break;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    if (tries++ >= RetryCount)
                        throw;

                    OnRetryStarted(this, new FileOperationRetryStartedEventArgs(RetryCount, tries, RetryInterval, e));
                    Thread.Sleep(RetryInterval);
                }
            }
        }

        protected void TryMoveFile(FileInfo source, FileInfo target)
        {
            var tries = 0;
            while (true)
            {
                try
                {
                    FileUtils.MoveFile(source, target, BufferSize, WhatToCopy, ProgressHandler, cancellation);
                    lock (SynchronizingObject)
                    {
                        Statistics.TotalFiles++;
                        Statistics.BytesTransferred += target.Length;
                    }
                    break;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    if (tries++ >= RetryCount)
                        throw;

                    OnRetryStarted(this, new FileOperationRetryStartedEventArgs(RetryCount, tries, RetryInterval, e));
                    Thread.Sleep(RetryInterval);
                }
            }
        }

        #endregion

        #region Private methods

        protected void ProgressHandler(FileInfo source, FileInfo destination, long copied, long chunkSize)
        {
            OnChunkFinished(this, new FileOperationProgressEventArgs(source, destination, copied, chunkSize));
        }

        #endregion
    }
}
