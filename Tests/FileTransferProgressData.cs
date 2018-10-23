using System;

namespace Tests
{
    public struct FileTransferProgressData : IProgressTracker
    {
        public long FileSize { get; set; }
        public int ChunkSize { get; set; }
        public long BytesTransferred{ get; set; }
        public TimeSpan Elapsed { get; set; }
        public double PercentComplete => (double) BytesTransferred / FileSize;

        public FileTransferProgressData(long fileSize, int chunkSize, long bytesTransferred, TimeSpan elapsed) 
            : this()
        {
            FileSize = fileSize;
            ChunkSize = chunkSize;
            BytesTransferred = bytesTransferred;
            Elapsed = elapsed;
        }

        public double GetTransferRatePerSecond(TransferRateUnitOfMeasure uom)
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetEstimatedTimeRemaining(double speed, TransferRateUnitOfMeasure uom)
        {
            throw new NotImplementedException();
        }
    }
}