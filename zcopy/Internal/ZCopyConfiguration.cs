using System;
using BananaHomie.ZCopy.Logging;

namespace BananaHomie.ZCopy.Internal
{
    internal static class ZCopyConfiguration
    {
        public static TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMilliseconds(1000);
        public static bool UseUtc { get; set; }
        public static bool DisableAnsiConsole { get; set; }
        public  static ZCopyEnvironment Environment { get; set; } = new ZCopyEnvironment();
        public static CopySpeedUomTypes CopySpeedUom { get; set; } = CopySpeedUomTypes.Megabits;      
    }
}
