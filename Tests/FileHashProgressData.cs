using System;

namespace Tests
{
    public struct FileHashProgressData : IProgressTracker
    {
        public FileHashProgressData(long fileSize, int blockSize, int bytesHashed, TimeSpan elapsed) : this()
        {
            FileSize = fileSize;
            BlockSize = blockSize;
            BytesHashed = bytesHashed;
            Elapsed = elapsed;
        }

        public long FileSize { get; set; }
        public int BlockSize { get; set; }
        public int BytesHashed { get; set; }
        public TimeSpan Elapsed { get; set; }

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