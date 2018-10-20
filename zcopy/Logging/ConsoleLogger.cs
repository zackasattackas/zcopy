﻿using BananaHomie.ZCopy.Internal;
using BananaHomie.ZCopy.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BananaHomie.ZCopy.AnsiConsole;
using BananaHomie.ZCopy.AnsiConsole.Extensions;
using McMaster.Extensions.CommandLineUtils;

namespace BananaHomie.ZCopy.Logging
{
    internal class ConsoleLogger : ICopyProgressLogger
    {
        #region Fields

        private readonly Mutex handlerControl;
        private readonly object sync;
        private string OutputFormat = "{0,-50} {1,10} {2,10} {3,4} {4,12} {5,10} {6,10}";
        private string currentFile;
        private string currentDir;
        private Stopwatch copyTimer;
        private Stopwatch hashTimer;
        private DateTime refreshed;
        private bool showVerificationStatus;
        private bool sourceVerificationComplete;
        private bool targetVerificationComplete;
        private bool verificationResult;
        private int currentCursorTop;
        private readonly CopySpeedUomTypes copySpeedUom;
        private MD5Verification verification;

        #endregion

        #region Properties

        private static int cursorTop => Console.CursorTop;

        #endregion

        #region Ctor

        public ConsoleLogger(CopySpeedUomTypes uom = CopySpeedUomTypes.Megabits)
        {
            handlerControl = new Mutex();
            sync = new object();
            copySpeedUom = uom;
        }

        #endregion

        #region Public methods

        public void Initialize(CommandLineApplication app, FileOperation fileOperation)
        {
            fileOperation.OperationStarted += FileOperationOnOperationStarted;
            fileOperation.ChunkFinished += FileOperationOnChunkFinished;
            fileOperation.OperationCompleted += FileOperationOnOperationCompleted;
            fileOperation.Error += FileOperationOnError;

            verification = fileOperation.Handlers.OfType<MD5Verification>().SingleOrDefault();
            if (verification != null)
            {
                fileOperation.Handlers.Remove(verification);
                verification.FileHashComputed += FileOperationOnFileHashComputed;
                verification.MD5VerificationStarted += FileOperationOnMd5VerificationStarted;
                verification.MD5VerificationFinished += FileOperationOnMd5VerificationFinished;
                fileOperation.Handlers.Add(verification);
            }

            if (fileOperation is FileCopy copy && copy.Options.HasFlag(CopyOptions.VerifyMD5) ||
                fileOperation is FileMove move && move.Options.HasFlag(MoveOptions.VerifyMD5))
            {
                showVerificationStatus = true;
                OutputFormat += " {7}";
            }

            Console.Out.WriteLine(OutputFormat, "File", "Size", "Copied", "Prog", "Speed", "Eta", "Elapsed", showVerificationStatus ? "MD5" : null);
            copyTimer = new Stopwatch();
            hashTimer = new Stopwatch();
        }

        public void Deinitialize(CommandLineApplication app, FileOperation fileOperation)
        {
            handlerControl?.Dispose();
        }

        #endregion

        #region Event handlers

        private void FileOperationOnOperationStarted(object sender, FileOperationStartedEventArgs e)
        {
            handlerControl.WaitOne();
            currentFile = e.FullPath.Replace("\\\\?\\UNC", "\\");
            sourceVerificationComplete = false;
            targetVerificationComplete = false;
            verificationResult = false;

            var dir = Path.GetDirectoryName(currentFile);
            if (currentDir != dir)
            {
                currentDir = dir;
                Console.Out.WriteLine($"\r\n\r\n    Directory: {currentDir}");
            }

            Console.SetCursorPosition(0, currentCursorTop = cursorTop + 1);
            copyTimer.Restart();
            handlerControl.ReleaseMutex();
        }

        private void FileOperationOnError(object sender, FileOperationErrorEventArgs e)
        {
            handlerControl.WaitOne();
            lock (sync)
                Utilities.Print((e.Exception.Message + " " + e.Exception.InnerException?.Message).TrimEnd('\r', '\n'),
                    color: ConsoleColor.Red, newLine: false);
            handlerControl.ReleaseMutex();
        }

        private void FileOperationOnOperationCompleted(object sender, FileOperationCompletedEventArgs e)
        {
            // Not used currently
        }

        private void FileOperationOnChunkFinished(object sender, FileOperationProgressEventArgs e)
        {
            handlerControl.WaitOne();

            currentCursorTop = cursorTop;

            if (DateTime.Now - refreshed < ZCopyConfiguration.RefreshInterval && e.PercentComplete < 100.00)
                return;

            var (speedBase, uom) = Helpers.GetCopySpeedBase(ZCopyConfiguration.CopySpeedUom);
            var speed = Helpers.GetCopySpeed(e.BytesCopied, speedBase, copyTimer.Elapsed - hashTimer.Elapsed);
            var elapsed = copyTimer.ElapsedMilliseconds < 1000 ? "<1s" : copyTimer.Elapsed.ToString("hh\\hmm\\mss\\s");

            Print(string.Format(OutputFormat,
                TruncateFileName(Path.GetFileName(currentFile), 50),
                new FileSize(e.SourceFile.Length),
                new FileSize(e.BytesCopied),
                PercentComplete(),
                Helpers.CopySpeedToString(uom, speed).PadLeft(uom.Length < 3 ? 10 : 11),
                Helpers.EtaToString(Helpers.GetTimeRemaining(e.SourceFile.Length, e.BytesCopied, speed, speedBase)),
                elapsed,
                VerificationStatus()?.PadRight(10)), false);

            Console.SetCursorPosition(0, currentCursorTop);

            refreshed = DateTime.Now;

            handlerControl.ReleaseMutex();

            #region Local functions

            string TruncateFileName(string value, int maxLength)
            {
                if (value.Length <= maxLength)
                    return value;

                return value.Substring(0, maxLength / 2 - 3) + "..." + value.Substring(value.Length - maxLength / 2);
            }

            string PercentComplete()
            {
                return e.PercentComplete.ToString("N0") + '%';
            }

            string VerificationStatus()
            {
                if (!showVerificationStatus)
                    return null;

                if (!sourceVerificationComplete) return "Waiting";

                if (targetVerificationComplete)
                    return (verificationResult ? "OK".ColorText(EscapeCodes.ForegroundGreen) : "Failed".ColorText(EscapeCodes.ForegroundRed)) + EscapeCodes.EraseCharacters(8);

                return "Verifying";
            }

            #endregion
        }

        private void FileOperationOnFileHashComputed(object sender, FileHashComputedEventArgs e)
        {
            //handlerControl.WaitOne();

            if (e.FilePath == currentFile)
                sourceVerificationComplete = true;
            else
                targetVerificationComplete = true;

            //handlerControl.ReleaseMutex();
        }

        private void FileOperationOnMd5VerificationStarted(object sender, EventArgs e)
        {
            //handlerControl.WaitOne();
            hashTimer.Restart();
            //handlerControl.ReleaseMutex();
        }

        private void FileOperationOnMd5VerificationFinished(object sender, MD5VerificationFinishedEventArgs e)
        {
            //handlerControl.WaitOne();
            hashTimer.Stop();
            verificationResult = e.Successful;
            //handlerControl.ReleaseMutex();
        }

        #endregion

        #region Private methods

        private void Print(string message, bool newLine = true)
        {
            if (newLine)
                message += "\r\n";

            lock (sync) Console.Out.Write(message);
        }

        #endregion
    }
}