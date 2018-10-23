using System;

namespace Tests
{
    public class FileCopyRetrySettings
    {
        internal bool Retry { get; } = true;
        public int RetryCount { get; set; }
        public TimeSpan RetryInterval { get; set; }

        public static FileCopyRetrySettings None => new FileCopyRetrySettings();

        private FileCopyRetrySettings()
            : this(0, TimeSpan.Zero)
        {
            Retry = false;
        }

        public FileCopyRetrySettings(int retryCount, TimeSpan retryInterval)
        {
            RetryCount = retryCount;
            RetryInterval = retryInterval;
        }
    }
}