using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class FileCopy : IDisposable
    {
        #region Fields

        private List<byte> sourceHash;
        private Stopwatch stopwatch;

        #endregion

        #region Properties

        public FileInfo Source { get; }
        public FileInfo Destination { get; }
        public int BufferSize { get; set; } = 4096;
        public HashAlgorithm HashAlgorithm { get; set; }
        public FileCopyOptions Options { get; }
        public FileCopyRetrySettings RetrySettings { get; set; } = FileCopyRetrySettings.None;

        #endregion

        #region Ctor

        public FileCopy(FileInfo source, FileInfo destination, FileCopyOptions options = FileCopyOptions.None)
        {
            Source = source;
            Destination = destination;
            Options = options;

            sourceHash = new List<byte>();
            stopwatch = new Stopwatch();
        }

        #endregion

        #region Events

        public event EventHandler<FileCopyStartedEventArgs2> Started;
        public event EventHandler<FileTransferProgressEventArgs> ChunkTransferred;                
        public event EventHandler<FileHashProgressEventArgs> ChunkHashed;
        public event EventHandler<FileHashMismatchEventArgs> HashFailed; 
        public event EventHandler<FileHashComputedEventArgs2> HashComputed; 
        public event EventHandler<EventArgs> Completed;
        public event EventHandler<FileCopyRetryStartedEventArgs> RetryStarted; 

        #endregion

        #region Public methods

        public void Copy()
        {
            CopyAsync().Wait();
        }

        public async Task CopyAsync(CancellationToken cancellation = default)
        {
            if (Options.HasFlag(FileCopyOptions.VerifyFileHash) && HashAlgorithm == default)
                throw new Exception($"The {FileCopyOptions.VerifyFileHash} bit flag was set, but no hashing algorithm was specified.");

            stopwatch.Start();
            OnStarted(this, new FileCopyStartedEventArgs2(Source, Destination));
            try
            {
                using (var freader = OpenSource())
                using (var fwriter = OpenDestination())
                {
                    if (freader.Position != fwriter.Position)
                        freader.Position = fwriter.Position;

                    var inputBuffer = new byte[BufferSize];
                    var bytesWritten = 0;
                    var tries = 0;

                    while (true)
                    {
                        try
                        {
                            int bytesRead;
                            while (0 != (bytesRead = await freader.ReadAsync(inputBuffer, 0, inputBuffer.Length, cancellation))
                                   && !cancellation.IsCancellationRequested)
                            {                        
                                cancellation.ThrowIfCancellationRequested();
                                bytesWritten += await CopyBytesAsync(inputBuffer, bytesRead, fwriter);

                                if (Options.HasFlag(FileCopyOptions.VerifyFileHash))
                                {
                                    HashAlgorithm.TransformBlock(inputBuffer, 0, bytesRead, inputBuffer, 0);
                                    OnChunkHashed(this, new FileHashProgressEventArgs(Source, new FileHashProgressData(Source.Length, bytesRead, sourceHash.Count, stopwatch.Elapsed)));
                                }

                                OnChunkTransferred(this, new FileTransferProgressEventArgs(Source, Destination, new FileTransferProgressData(Source.Length, bytesRead, bytesWritten, stopwatch.Elapsed)));                                  
                            }


                            if (Options.HasFlag(FileCopyOptions.VerifyFileHash))
                            {
                                HashAlgorithm.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
                                sourceHash = HashAlgorithm.Hash.ToList();
                            }


                            break;
                        }                         
                        catch (Exception e)
                        {
                            if (e is OperationCanceledException)
                                throw;
                            if (!RetrySettings.Retry || tries >= RetrySettings.RetryCount)
                                throw;

                            OnRetryStarted(this, new FileCopyRetryStartedEventArgs(tries++, RetrySettings.RetryInterval, e));
                        } 
                    }                  
                }

                cancellation.ThrowIfCancellationRequested();

                if (Options.HasFlag(FileCopyOptions.VerifyFileHash))
                {
                    stopwatch.Reset();
                    Destination.Refresh();
                    EnsureChecksumsMatch(cancellation);
                }
            }
            catch (OperationCanceledException)
            {
                if (!Options.HasFlag(FileCopyOptions.Restartable))
                    Destination.Delete();

                throw;
            }   
            
            if (Options.HasFlag(FileCopyOptions.DeleteSourceOnSuccessfulCopy))
                Source.Delete();

            OnCompleted(this, EventArgs.Empty);
        }

        #endregion

        #region IDisposable methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
                HashAlgorithm?.Dispose();
        }

        #endregion

        #region Protected methods

        protected void OnStarted(object sender, FileCopyStartedEventArgs2 args)
        {
            Started?.Invoke(sender, args);
        }

        protected void OnChunkTransferred(object sender, FileTransferProgressEventArgs args)
        {
            ChunkTransferred?.Invoke(this, args);
        }

        protected void OnChunkHashed(object sender, FileHashProgressEventArgs args)
        {
            ChunkHashed?.Invoke(this, args);
        }

        protected void OnHashFailed(object sender, FileHashMismatchEventArgs args)
        {
            HashFailed?.Invoke(this, args);
        }

        protected void OnHashComputed(object sender, FileHashComputedEventArgs2 args)
        {
            HashComputed?.Invoke(this, args);
        }

        protected void OnCompleted(object sender, EventArgs args)
        {
            Completed?.Invoke(this, args);
        }

        protected void OnRetryStarted(object sender, FileCopyRetryStartedEventArgs args)
        {
            RetryStarted?.Invoke(sender, args);
        }

        #endregion

        #region Private methods

        private FileStream OpenSource()
        {
            var stream = Source.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            if (Destination.Exists && Options.HasFlag(FileCopyOptions.Restartable))
                stream.Seek(Destination.Length - BufferSize, SeekOrigin.Begin);

            return stream;
        }

        private FileStream OpenDestination()
        {
            if (!Destination.Exists || !Options.HasFlag(FileCopyOptions.Restartable))
                return Destination.Open(FileMode.Create, FileAccess.Write, FileShare.None);

            var stream = Destination.Open(FileMode.Open, FileAccess.Write, FileShare.None);
            stream.Seek(BufferSize, SeekOrigin.End);

            return stream;
        }

        private static async Task<int> CopyBytesAsync(byte[] buffer, int count,FileStream outputStream)
        {
            await outputStream.WriteAsync(buffer, 0, count);
            return buffer.Length;
        }

        private async void EnsureChecksumsMatch(CancellationToken cancellation = default)
        {
            HashAlgorithm.Initialize();

            using (var freader = Destination.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var inputBuffer = new byte[BufferSize];
                var bytesHashed = 0;

                try
                {
                    int bytesRead;
                    while (0 != (bytesRead = await freader.ReadAsync(inputBuffer, 0, inputBuffer.Length, cancellation)))
                    {
                        cancellation.ThrowIfCancellationRequested();
                        HashAlgorithm.TransformBlock(inputBuffer, 0, bytesRead, inputBuffer, 0);
                        OnChunkHashed(this,
                            new FileHashProgressEventArgs(Destination,
                                new FileHashProgressData(Destination.Length, bytesRead, bytesHashed += bytesRead, stopwatch.Elapsed)));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            for (var i = 0; i < sourceHash.Count; i++)
            {
                if (sourceHash[i] != HashAlgorithm.Hash[i])
                    OnHashFailed(this, new FileHashMismatchEventArgs(Destination, i, sourceHash[i], HashAlgorithm.Hash[i]));

                cancellation.ThrowIfCancellationRequested();
            }

            OnHashComputed(this, new FileHashComputedEventArgs2(HashAlgorithm.Hash));
        }

        #endregion
    }
}
