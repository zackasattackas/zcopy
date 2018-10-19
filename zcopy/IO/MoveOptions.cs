using System;

namespace BananaHomie.ZCopy.IO
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
