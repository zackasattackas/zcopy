using System;

namespace BananaHomie.ZCopy.FileOperations
{
    public class FileOperationRetryStartedEventArgs : EventArgs
    {
        public FileOperationRetryStartedEventArgs(int maxRetries, int retryCount, TimeSpan retryInterval, Exception reason)
        {
            MaxRetries = maxRetries;
            RetryCount = retryCount;
            RetryInterval = retryInterval;
            Reason = reason;
        }

        public int MaxRetries { get; }
        public int RetryCount { get;}
        public TimeSpan RetryInterval { get; }
        public Exception Reason { get; set; }
    }
}