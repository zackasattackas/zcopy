using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using BananaHomie.ZCopy.FileOperations;
using BananaHomie.ZCopy.Logging;

namespace BananaHomie.ZCopy.Internal
{
    internal static class Helpers
    {
        public static void ThrowLastWin32Exception()
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static (double Base, string Uom) GetCopySpeedBase(CopySpeedUomTypes uom)
        {
            switch (uom)
            {
                case CopySpeedUomTypes.Bytes:
                    return (1, "B");
                case CopySpeedUomTypes.Bits:
                    return (FileSize.OneBit, "b");
                case CopySpeedUomTypes.Kilobits:
                    return (FileSize.OneKilobit, "Kb");
                case CopySpeedUomTypes.Kilobytes:
                    return (FileSize.OneKilobyte, "KB");
                case CopySpeedUomTypes.Kibibytes:
                    return (FileSize.OneKibibyte, "KiB");
                case CopySpeedUomTypes.Megabits:
                    return (FileSize.OneMegabit, "Mb");
                case CopySpeedUomTypes.Megabytes:
                    return (FileSize.OneMegabyte, "MB");
                case CopySpeedUomTypes.Mebibytes:
                    return (FileSize.OneMebibyte, "MiB");
                case CopySpeedUomTypes.Gigabits:
                    return (FileSize.OneGigabit, "Gb");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static double GetCopySpeed(long bytesCopied, double baseValue, TimeSpan elapsed)
        {
            return bytesCopied / baseValue / elapsed.TotalSeconds;
        }

        public static string CopySpeedToString(string uom, double speed)
        {
            return $"{(speed >= 10000 ? "\u221E" : speed.ToString("N1"))}" + $" {uom}/s";
        }

        public static TimeSpan GetTimeRemaining(long totalSize, long bytesCopied, double speed, double baseValue)
        {
            var seconds = ((double) totalSize - bytesCopied) / baseValue / speed;
            return TimeSpan.FromSeconds(double.IsNaN(seconds) ? 0 : seconds);
        }

        public static string EtaToString(TimeSpan eta)
        {
            return eta.TotalMilliseconds < 1000 ? "<1s" : eta.ToString("hh\\hmm\\mss\\s");
        }

        public static bool Equals(FileInfo x, FileInfo y, WhatToCopy whatToCompare)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            if (ReferenceEquals(x, y))
                return true;
            if (x.Length != y.Length)
                return false;
            if (x.Name != y.Name)
                return false;
            if (x.Extension != y.Extension)
                return false;
            if (whatToCompare.HasFlag(WhatToCopy.Attributes))
            {
                if (x.Attributes != y.Attributes)
                    return false;
            }
            if (whatToCompare.HasFlag(WhatToCopy.Timestamps))
            {
                if (x.CreationTime != y.CreationTime)
                    return false;
                if (x.LastWriteTime != y.LastWriteTime)
                    return false;
            }
            if (whatToCompare.HasFlag(WhatToCopy.Security))
            {
                if (!x.GetAccessControl().Equals(y.GetAccessControl()))
                    return false;
            }

            return true;
        }
    }
}