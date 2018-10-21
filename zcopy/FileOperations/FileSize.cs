using System;

namespace BananaHomie.ZCopy.FileOperations
{
    public struct FileSize
    {
        public const double OneBit = 0.125D;
        public const short OneKilobit = 125;
        public const int OneKilobyte = 1024;
        public const double OneKibibyte = 1048.576D;
        public const int OneMegabit = 125000;
        public const int OneMegabyte = 1048576;
        public const double OneMebibyte = 1073741.824D;
        public const int OneGigabit = 128000000;
        public const int OneGigabyte = 1073741824;
        public const long OneTerabyte = 1099511627776L;

        public long Bytes { get; private set; }

        public static FileSize operator + (FileSize left, FileSize right)
        {
            left.Bytes += right.Bytes;

            return left;
        }

        public static FileSize operator -(FileSize left, FileSize right)
        {
            left.Bytes -= right.Bytes;

            return left;
        }

        public static implicit operator long(FileSize value)
        {
            return value.Bytes;
        }

        public FileSize(long bytes)
        {
            Bytes = bytes;
        }

        public override string ToString()
        {
            return ToString(Bytes);
        }

        private static string ToString(long size)
        {
            double value;
            string unit;
            var absolute = Math.Abs(size);

            if (absolute >= OneTerabyte)
            {
                value = (double)size / OneTerabyte;
                unit = "TB";
            }
            else if(absolute >= OneGigabyte)
            {
                value = (double)size / OneGigabyte;
                unit = "GB";
            }
            else if (absolute >= OneMegabyte)
            {
                value = (double)size / OneMegabyte;
                unit = "MB";
            }
            else if (absolute >= OneKilobyte)
            {
                value = (double)size / OneKilobyte;
                unit = "KB";
            }
            else
            {
                value = size;
                unit = "B";
            }

            return $"{value:N1} {unit,-2}";
        }
    }
}