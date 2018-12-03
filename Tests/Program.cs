using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var source = new FileInfo("E:\\backup\\spx\\zack-desktop\\C_VOL-b002-i002.spi");
            var destination = new FileInfo("D:\\C_VOL-b002-i002.spi");
            var options = FileCopyOptions.VerifyFileHash | FileCopyOptions.Restartable;

            try
            {
                using (var fileCopy = new FileCopy(source, destination, options) )
                {
                    fileCopy.HashAlgorithm = new MD5Cng();
                    //fileCopy.Started += FileCopyOnStarted;
                    //fileCopy.ChunkTransferred += FileCopyOnChunkTransferred;
                    //fileCopy.ChunkHashed += FileCopyOnChunkHashed;
                    //fileCopy.HashFailed += FileCopyOnHashFailed;
                    //fileCopy.HashComputed += FileCopyOnHashComputed;
                    //fileCopy.Completed += FileCopyOnCompleted;

                    fileCopy.Copy();
                }
            }
            catch (Exception e)
            {
                Debugger.Break();
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
