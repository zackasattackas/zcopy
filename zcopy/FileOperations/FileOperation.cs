using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using BananaHomie.ZCopy.FileSystemSearch;
using BananaHomie.ZCopy.Internal;

// ReSharper disable InconsistentNaming
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global

namespace BananaHomie.ZCopy.FileOperations
{
    public abstract class FileOperation : IDisposable
    {
        #region Fields

        protected WindowsImpersonationContext impersonationContext;
        // protected string currentFile;
        protected CancellationToken cancellation;
        private NetworkCredential credentials;

        #endregion

        #region Properties

        public List<IFileOperationHandler> Handlers { get; }
        public DirectoryInfo Source { get; set; }
        public DirectoryInfo Destination { get; set; }
        public List<ISearchFilter> FileFilters { get; set; }
        public List<ISearchFilter> DirectoryFilters { get; set; }
        public int BufferSize { get; set; }
        public FileCopyStats Statistics { get; protected set; }
        public TimeSpan InterPacketGap { get; set; }
        public TimeSpan StartTimeOfDay { get; set; }
        public TimeSpan StopTimeOfDay { get; set; }
        public WhatToCopy WhatToCopy { get; set; } = WhatToCopy.Data;

        public NetworkCredential Credentials
        {
            get => credentials;
            set
            {
                impersonationContext?.Undo();
                impersonationContext?.Dispose();
                impersonationContext = Utilities.ImpersonateUser(value);
                credentials = value;
            }
        }

        #endregion

        #region Events

        public event EventHandler<FileOperationStartedEventArgs> OperationStarted;
        public event EventHandler<FileOperationProgressEventArgs> ChunkFinished;
        public event EventHandler<FileOperationCompletedEventArgs> OperationCompleted;
        public event EventHandler<FileOperationErrorEventArgs> Error;

        #endregion

        #region Ctor

        protected FileOperation(DirectoryInfo source, DirectoryInfo destination, List<ISearchFilter> fileFilters, List<ISearchFilter> directoryFilters)
        {
            Source = source;
            Destination = destination;
            FileFilters = fileFilters;
            DirectoryFilters = directoryFilters;
            Statistics = new FileCopyStats();
            Handlers = new List<IFileOperationHandler>();
        }

        #endregion

        #region Public methods

        public abstract void Invoke(CancellationToken cancellationToken = default);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Internal methods

        internal abstract string GetOptionsString();

        #endregion

        #region Protected methods

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                impersonationContext?.Undo();
                impersonationContext?.Dispose();
            }
        }

        protected void OnOperationStarted(object sender, FileOperationStartedEventArgs args)
        {
            OperationStarted?.Invoke(sender, args);
        }

        protected void OnChunkFinished(object sender, FileOperationProgressEventArgs args)
        {
            ChunkFinished?.Invoke(sender, args);
        }

        protected void OnCompleted(object sender, FileOperationCompletedEventArgs args)
        {
            OperationCompleted?.Invoke(sender, args);
        }

        protected void OnError(object sender, FileOperationErrorEventArgs args)
        {
            Error?.Invoke(sender, args);
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

        #endregion

        #region Private methods

        protected void ProgressHandler(FileInfo source, FileInfo destination, long copied, long chunkSize)
        {
            OnChunkFinished(this, new FileOperationProgressEventArgs(source, destination, copied, chunkSize));
        }

        #endregion
    }
}
