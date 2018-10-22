using System;

namespace BananaHomie.ZCopy.FileOperations
{
    [Flags]
    public enum FileOperationOptions
    {
        None = 0,
        Recurse = 1,
        Restartable = 2,
        CopySymbolicLink = 4,
        Mirror = 8,
        VerifyMD5 = 16
    }
}