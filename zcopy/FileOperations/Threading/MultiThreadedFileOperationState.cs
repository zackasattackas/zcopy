using System;
using System.IO;
using System.Threading;

namespace BananaHomie.ZCopy.FileOperations.Threading
{
    public class MultiThreadedFileOperationState : IDisposable
    {
        public Guid Guid { get; }
        public AutoResetEvent WaitHandle { get; }
        public bool IsReady { get; set; }
        public FileInfo SourceFile { get; set; }
        public FileInfo TargetFile { get; set; }
        public bool IsCompleted { get; set; }

        public MultiThreadedFileOperationState()
        {
            Guid = Guid.NewGuid();
            WaitHandle = new AutoResetEvent(false);
            IsReady = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
                WaitHandle?.Dispose();
        }
    }
}