using System;

namespace Tests
{
    [Flags]
    public enum FileCopyOptions
    {
        None = 0,
        Restartable = 1,
        DeleteSourceOnSuccessfulCopy = 2,
        VerifyFileHash = 4
    }
}