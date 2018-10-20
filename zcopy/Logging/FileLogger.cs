using BananaHomie.ZCopy.Commands;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using BananaHomie.ZCopy.FileOperations;

// ReSharper disable InconsistentNaming

namespace BananaHomie.ZCopy.Logging
{
    internal class FileLogger : ICopyProgressLogger
    {
        private readonly FileInfo logFile;
        private bool append;
        private bool isCopy;

        public FileLogger(string filePath, bool append)
        {
            logFile = new FileInfo(filePath);
            this.append = append;
        }

        public void Initialize(CommandLineApplication app, FileOperation fileOperation)
        {
            isCopy = fileOperation is FileCopy;
            fileOperation.OperationStarted += FileOperationOnOperationStarted;
            fileOperation.OperationCompleted += FileOperationOnOperationCompleted;
            fileOperation.ChunkFinished += FileOperationOnChunkFinished;
            fileOperation.Error += FileOperationOnError;

            var md5Verification = fileOperation.Handlers.OfType<MD5Verification>().SingleOrDefault();
            if (md5Verification != null)
            {
                md5Verification.FileHashComputed += MD5VerificationOnFileHashComputed;
                md5Verification.MD5VerificationStarted += MD5VerificationOnMD5VerificationStarted;
                md5Verification.MD5VerificationFinished += MD5VerificationOnMD5VerificationFinished;
            }

            if (!logFile.Exists)
                logFile.Create().Dispose();
            else
                Write("\r\n\r\n");

            if (!Console.IsOutputRedirected)
                ZCopyCommand.PrintHeader(app, fileOperation);
        }
        public void Deinitialize(CommandLineApplication app, FileOperation operation)
        {
            if (!Console.IsOutputRedirected)
                ZCopyCommand.PrintFooter(operation);
        }

        private void MD5VerificationOnFileHashComputed(object sender, FileHashComputedEventArgs e)
        {
            Write($"Finished computing MD5 hash of {Path.GetFileName(e.FilePath)}. Result: {e.MD5Hash}");
        }

        private void MD5VerificationOnMD5VerificationStarted(object sender, EventArgs e)
        {
            Write("Starting MD5 verification");
        }

        private void MD5VerificationOnMD5VerificationFinished(object sender, MD5VerificationFinishedEventArgs e)
        {
            Write($"Verification {(e.Successful ? "succeeded." : "failed!")} Checksum: {e.Checksum}");
        }

        private void FileOperationOnOperationStarted(object sender, FileOperationStartedEventArgs e)
        {
            Write(e.FullPath);
        }

        private void FileOperationOnOperationCompleted(object sender, FileOperationCompletedEventArgs e)
        {
            // Not used currently
        }

        private void FileOperationOnChunkFinished(object sender, FileOperationProgressEventArgs e)
        {
            // Not used currently
        }

        private void FileOperationOnError(object sender, FileOperationErrorEventArgs e)
        {
            Write($"ERROR: {e.Exception.Message + " " + e.Exception.InnerException?.Message}");
        }

        private void Write(string value, bool newLine = true)
        {
            if (newLine)
                value += "\r\n";

            using (var fwriter = new StreamWriter(logFile.OpenWrite()))
                fwriter.Write(value);
        }
    }
}