using System;

namespace BananaHomie.ZCopy.IO
{
    [Flags]
    public enum WhatToCopy
    {
        None = 0,
        Data = 1,
        Timestamps = 2,
        Attributes = 4,
        Security = 8
    }
}
