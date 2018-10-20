using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class FileAttributeFilter : ISearchFilter
    {
        public FileAttributes Attributes { get; }
        public bool Exclude { get; }

        public FileAttributeFilter(FileAttributes attributes, bool exclude)
        {
            Attributes = attributes;
            Exclude = exclude;
        }

        public static FileAttributeFilter Parse(IEnumerable<char> values)
        {
            FileAttributes attributes = 0;
            bool exclude = default;
            foreach (var value in values)
                switch (value)
                {
                    case 'A':
                        attributes |= FileAttributes.Archive;
                        break;
                    case 'C':
                        attributes |= FileAttributes.Compressed;
                        break;
                    case 'E':
                        attributes |= FileAttributes.Encrypted;
                        break;
                    case 'H':
                        attributes |= FileAttributes.Hidden;
                        break;
                    case 'O':
                        attributes |= FileAttributes.Offline;
                        break;
                    case 'R':
                        attributes |= FileAttributes.ReparsePoint;
                        break;
                    case 'S':
                        attributes |= FileAttributes.System;
                        break;
                    case '!':
                        exclude = true;
                        break;
                }

            return new FileAttributeFilter(attributes, exclude);
        }

        public bool IsMatch(FileSystemInfo item)
        {
            return Exclude ? (item.Attributes & Attributes) == 0 : (item.Attributes & Attributes) > 0;
        }
    }
}
