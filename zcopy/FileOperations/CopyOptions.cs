using System;

namespace BananaHomie.ZCopy.FileOperations
{
    [Flags]
    public enum CopyOptions
    {
        None = 0,
        Recurse = 1,
        Restartable = 2,
        CopySymbolicLink = 4,
        Mirror = 8,
        // ReSharper disable once InconsistentNaming
        VerifyMD5 = 16
    }
}