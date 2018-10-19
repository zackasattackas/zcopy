namespace BananaHomie.ZCopy.IO
{
    public class FileHashComputedEventArgs
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