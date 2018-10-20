using System;

namespace BananaHomie.ZCopy.FileOperations
{
    public class FileHashComputedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public string MD5Hash { get; }

        public FileHashComputedEventArgs(string filePath, string md5Hash)
        {
            FilePath = filePath;
            MD5Hash = md5Hash;
        }
    }
}