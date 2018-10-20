using BananaHomie.ZCopy.AnsiConsole;
using BananaHomie.ZCopy.AnsiConsole.Extensions;
using BananaHomie.ZCopy.Internal;
using BananaHomie.ZCopy.IO;
using BananaHomie.ZCopy.IO.Threading;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace BananaHomie.ZCopy.Logging
{
    internal class MultiThreadedConsoleLogger : ICopyProgressLogger
    {
        #region Fields

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

        public MultiThreadedConsoleLogger(CopySpeedUomTypes uom = CopySpeedUomTypes.Megabits)
        {
            displayUom = uom;
        }

        #endregion

        #region Public methods

        public void Initialize(CommandLineApplication app, FileOperation operation)
        {
            var mtfo = (MultiThreadedFileOperation) operation;
            mtfo.OperationStarted += MtfoOnOperationStarted;
            mtfo.OperationCompleted += MtfoOnOperationCompleted;
            mtfo.ChunkFinished += MtfoOnChunkFinished;
            mtfo.Error += MtfoOnError;

            isCopy = mtfo is MultiThreadedFileCopy;
            cancellation = new CancellationTokenSource();
            stopwatch = Stopwatch.StartNew();

            ThreadPool.QueueUserWorkItem(ProgressThreadProc, cancellation.Token);
        }

        public void Deinitialize(CommandLineApplication app, FileOperation operation)
        {
            Console.Out.WriteLine("\u001b[1M\u001b[2A");
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

        private void MtfoOnChunkFinished(object sender, FileOperationProgressEventArgs e)
        {
            Interlocked.Add(ref bytesCopied, e.ChunkSize);
            Interlocked.Increment(ref sizeIncrements);
        }

        private void MtfoOnError(object sender, FileOperationErrorEventArgs e)
        {
            // Not used yet
        }

        #endregion

        #region Private methods

        private void ProgressThreadProc(object state)
        {
            var cancel = (CancellationToken) state;
            var progressFormat = new StringBuilder()
                .SavePosition()
                .Append("{0:N0} files(s) found | ")
                //.Append("{1:N0} active thread(s) | ")
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
                Console.Out.Write(progressFormat, fileCount, new FileSize(bytesCopied), Helpers.CopySpeedToString(uom, speed));

                Thread.Sleep(ZCopyConfiguration.RefreshInterval);
            }
        }

        #endregion
    }
}
