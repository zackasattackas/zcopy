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
            return TimeSpan.FromSeconds(Double.IsNaN(seconds) ? 0 : seconds);
        }

        public static string EtaToString(TimeSpan eta)
        {
            return eta.TotalMilliseconds < 1000 ? "<1s" : eta.ToString("hh\\hmm\\mss\\s");
        }

        public static void Print(string value, bool newLine = true, ConsoleColor? color = null)
        {
            ConsoleColor current = default;
            if (color.HasValue)
            {
                current = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
            }

            if (newLine)
                value += Environment.NewLine;

            Console.Out.Write(value);

            if (color.HasValue)
                Console.ForegroundColor = current;
        }
    }
}