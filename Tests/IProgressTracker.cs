using System;

namespace Tests
{
    public interface IProgressTracker
    {
        double GetTransferRatePerSecond(TransferRateUnitOfMeasure uom);
        TimeSpan GetEstimatedTimeRemaining(double speed, TransferRateUnitOfMeasure uom);
    }
}