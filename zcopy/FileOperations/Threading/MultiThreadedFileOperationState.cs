using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BananaHomie.ZCopy.FileOperations.Threading
{
    public class MultiThreadedFileOperationState //: IDisposable
    {
        public MultiThreadedFileOperationState(FileInfo sourceFile, FileInfo targetFile)
        {
            SourceFile = sourceFile;
            TargetFile = targetFile;
        }

        //public Guid Guid { get; }
        //public AutoResetEvent WaitHandle { get; }
        //public bool IsReady { get; set; }
        public FileInfo SourceFile { get; set; }
        public FileInfo TargetFile { get; set; }

        //public bool IsCompleted { get; set; }

        //public MultiThreadedFileOperationState()
        //{
        //    Guid = Guid.NewGuid();
        //    WaitHandle = new AutoResetEvent(false);
        //    IsReady = true;
        //    Queue = new ConcurrentQueue<(FileInfo Source, FileInfo Target)>();            
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //protected void Dispose(bool disposing)
        //{
        //    if (disposing)
        //        WaitHandle?.Dispose();
        //}
    }
}