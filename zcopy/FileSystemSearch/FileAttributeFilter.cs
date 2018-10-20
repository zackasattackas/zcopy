using System.Collections.Generic;
using System.IO;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class FileAttributeFilter : ISearchFilter
    {
        public FileAttributes Attributes { get; }
        public bool Exclude { get; }

        public FileAttributeFilter(IEnumerable<char> values)
        {
            foreach (var value in values)
                switch (value)
                {
                    case 'A':
                        Attributes |= FileAttributes.Archive;
                        break;
                    case 'C':
                        Attributes |= FileAttributes.Compressed;
                        break;
                    case 'E':
                        Attributes |= FileAttributes.Encrypted;
                        break;
                    case 'H':
                        Attributes |= FileAttributes.Hidden;
                        break;
                    case 'O':
                        Attributes |= FileAttributes.Offline;
                        break;
                    case 'R':
                        Attributes |= FileAttributes.ReparsePoint;
                        break;
                    case 'S':
                        Attributes |= FileAttributes.System;
                        break;
                    case '!':
                        Exclude = true;
                        break;
                }
        }

        public bool IsMatch(FileSystemInfo item)
        {
            return Exclude ? (item.Attributes & Attributes) == 0 : (item.Attributes & Attributes) > 0;
        }
    }
}
