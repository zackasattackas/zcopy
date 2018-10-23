using System;

namespace Tests
{
    public class FileCopyRetryStartedEventArgs : EventArgs
    {
        public int TotalRetries { get; }
        public TimeSpan Interval { get; }
        public Exception Reason { get; }

        public FileCopyRetryStartedEventArgs(int totalRetries, TimeSpan interval, Exception reason)
        {
            TotalRetries = totalRetries;
            Interval = interval;
            Reason = reason;
        }
    }
}