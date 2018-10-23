using System;
using System.IO;

namespace Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var source = new FileInfo("");
            var destination = new FileInfo("");
            var options = FileCopyOptions.VerifyFileHash | FileCopyOptions.Restartable;
            
            using (var fileCopy = new FileCopy(source, destination, options))
            {
                fileCopy.Started += FileCopyOnStarted;
                fileCopy.ChunkTransferred += FileCopyOnChunkTransferred;
                fileCopy.ChunkHashed += FileCopyOnChunkHashed;
                fileCopy.HashFailed += FileCopyOnHashFailed;
                fileCopy.HashComputed += FileCopyOnHashComputed;
                fileCopy.Completed += FileCopyOnCompleted;

                fileCopy.Copy();
            }
        }

        private static void FileCopyOnStarted(object sender, FileCopyStartedEventArgs2 e)
        {
            throw new NotImplementedException();
        }

        private static void FileCopyOnChunkTransferred(object sender, FileTransferProgressEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void FileCopyOnChunkHashed(object sender, FileHashProgressEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void FileCopyOnHashFailed(object sender, FileHashMismatchEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void FileCopyOnHashComputed(object sender, FileHashComputedEventArgs2 e)
        {
            throw new NotImplementedException();
        }

        private static void FileCopyOnCompleted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
