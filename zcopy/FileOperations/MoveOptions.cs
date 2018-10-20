using System;

namespace BananaHomie.ZCopy.FileOperations
{
    [Flags]
    public enum MoveOptions
    {
        None = 0,
        Recurse =1,
        Restartable = 2,
        VerifyMD5 = 16
    }
}
