using BananaHomie.ZCopy.AnsiConsole;
using BananaHomie.ZCopy.AnsiConsole.Extensions;
using BananaHomie.ZCopy.Internal;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using BananaHomie.ZCopy.FileOperations;
using BananaHomie.ZCopy.FileOperations.Threading;

namespace BananaHomie.ZCopy.Logging
{
    internal class BasicConsoleLogger : ICopyProgressLogger
    {
        #region Fields

        private object lockObj;
        private int fileCount;
        private int filesBeingCopied;
        private long bytesCopied; // Support for up to 8 petabytes (Int64.MaxValue)
        private bool isCopy;
        private Stopwatch stopwatch;
        private CancellationTokenSource cancellation;
        private CopySpeedUomTypes displayUom;
        private int sizeIncrements;

        #endregion

        #region Ctor

        public BasicConsoleLogger()
        {
            lockObj = new object();
            displayUom = ZCopyConfiguration.CopySpeedUom;
        }

        #endregion

        #region Public methods

        public void Initialize(CommandLineApplication app, FileOperation operation)
        {
            operation.OperationStarted += MtfoOnOperationStarted;
            operation.OperationCompleted += MtfoOnOperationCompleted;
            operation.ChunkFinished += MtfoOnChunkFinished;
            operation.Error += MtfoOnError;
            //operation.RetryStarted += OperationOnRetryStarted;

            isCopy = operation is FileCopyOperation || operation is MultiThreadedFileCopy;
            cancellation = new CancellationTokenSource();
            stopwatch = Stopwatch.StartNew();

            ThreadPool.QueueUserWorkItem(ProgressThreadProc, cancellation.Token);
        }

        public void Deinitialize(CommandLineApplication app, FileOperation operation)
        {
            ZCopyOutput.Print("\u001b[1M\u001b[2A");
            cancellation?.Cancel();
            cancellation?.Dispose();
        }

        #endregion

        #region File operation event handlers

        private void MtfoOnOperationStarted(object sender, FileOperationStartedEventArgs e)
        {
            Interlocked.Increment(ref fileCount);
            Interlocked.Increment(ref filesBeingCopied);
        }

        private void MtfoOnOperationCompleted(object sender, FileOperationCompletedEventArgs e)
        {
            Interlocked.Decrement(ref filesBeingCopied);
        }

        private void MtfoOnChunkFinished(object sender, FileOperationChunkFinishedEventArgs e)
        {
            Interlocked.Add(ref bytesCopied, e.ChunkSize);
            Interlocked.Increment(ref sizeIncrements);
        }

        private void MtfoOnError(object sender, FileOperationErrorEventArgs e)
        {
            if (e.Exception is OperationCanceledException)
                return;
            
            lock (lockObj)
                ZCopyOutput.PrintError(e.Exception.Message + " " + e.Exception.InnerException?.Message);
        }

        //private void OperationOnRetryStarted(object sender, FileOperationRetryStartedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #region Private methods

        private void ProgressThreadProc(object state)
        {
            var cancel = (CancellationToken) state;
            var progressFormat = new StringBuilder()
                .SavePosition()
                .Append("{0:N0} files(s) found | ")
                .Append($"{{1,-4}} {(isCopy ? "copied" : "moved")} | ")
                .Append("{2,-6} (avg.)" + EscapeCodes.EraseCharacters(10))
                .RestorePosition()
                .ToString();

            // Wait for at least 1 file to be found 
            while (fileCount < 1 && !cancel.IsCancellationRequested)
                Thread.Sleep(500); 

            // Loop throughout the lifetime of the file operation, displaying average stats
            while (!cancel.IsCancellationRequested)
            {
                var (speedBase, uom) = Helpers.GetCopySpeedBase(ZCopyConfiguration.CopySpeedUom);
                var speed = Helpers.GetCopySpeed(bytesCopied, speedBase, stopwatch.Elapsed);
                lock (lockObj)
                    ZCopyOutput.Print(progressFormat, fileCount, new FileSize(bytesCopied), Helpers.CopySpeedToString(uom, speed));

                Thread.Sleep(ZCopyConfiguration.RefreshInterval);
            }
        }

        #endregion
    }
}
