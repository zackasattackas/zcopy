using System;
using System.IO;
using System.Threading;
using BananaHomie.ZCopy.Internal;
using BananaHomie.ZCopy.Internal.Extensions;

// ReSharper disable InconsistentNaming

namespace BananaHomie.ZCopy.FileOperations
{
    public class MD5Verification : IFileOperationHandler
    {
        #region Fields

        private FileHash sourceHash;
        private FileHash targetHash;
        private IAsyncResult sourceAsyncResult;
        private IAsyncResult targetAsyncResult;
        private string sourceMD5;
        private string targetMD5;

        #endregion

        #region Properties

        public WaitHandle WaitHandle { get; }

        #endregion

        #region Events

        public event EventHandler<EventArgs> MD5VerificationStarted;
        public event EventHandler<MD5VerificationFinishedEventArgs> MD5VerificationFinished;
        public event EventHandler<FileHashComputedEventArgs> FileHashComputed;

        #endregion

        #region Ctor

        public MD5Verification()
        {
            WaitHandle = new AutoResetEvent(false);
            sourceHash = new FileHash();
            targetHash = new FileHash();
        }

        #endregion

        #region Public methods

        public void OnPreProcessing(FileInfo source, FileInfo target)
        {
            Reset();

            if (source.IsShadowProtectImageFile())
            {
                var md5File = new FileInfo(source.FullName + ".md5");
                if (md5File.Exists)
                {
                    var content = source.ReadAllText();
                    if (!content.IsNullOrEmpty())
                        sourceMD5 = content.Split(' ')[0];
                }
            }

            if (string.IsNullOrEmpty(sourceMD5))
                sourceAsyncResult = sourceHash.BeginComputeMD5Hash(source, null, null);

            SignalCaller();
        }

        public void OnPostProcessing(FileInfo source, FileInfo target)
        {
            OnMD5VerificationStarted(this, EventArgs.Empty);
            sourceMD5 = sourceHash.EndComputeMD5Hash(sourceAsyncResult);
            OnFileHashComputed(this, new FileHashComputedEventArgs(source.FullName, sourceMD5));
            targetAsyncResult = targetHash.BeginComputeMD5Hash(target, null, null);
            targetMD5 = targetHash.EndComputeMD5Hash(targetAsyncResult);
            OnFileHashComputed(this, new FileHashComputedEventArgs(target.FullName, targetMD5));
            OnMD5VerificationFinished(this, new MD5VerificationFinishedEventArgs(sourceMD5 == targetMD5, sourceMD5));
            Reset();
            SignalCaller();
        }

        #endregion

        #region Protected methods

        protected void OnMD5VerificationStarted(object sender, EventArgs args)
        {
            MD5VerificationStarted?.Invoke(sender, args);
        }

        protected void OnMD5VerificationFinished(object sender, MD5VerificationFinishedEventArgs args)
        {
            MD5VerificationFinished?.Invoke(sender, args);
        }

        protected void OnFileHashComputed(object sender, FileHashComputedEventArgs args)
        {
            FileHashComputed?.Invoke(sender, args);
        }

        #endregion

        #region Private methods

        private void Reset()
        {
            sourceMD5 = null;
            targetMD5 = null;
            sourceAsyncResult = null;
            targetAsyncResult = null;
        }

        private void SignalCaller()
        {
            ((AutoResetEvent)WaitHandle).Set();
        }

        #endregion
    }
}