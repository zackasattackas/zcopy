namespace BananaHomie.ZCopy.IO
{
    public class FileOperationStartedEventArgs
    {
        public FileOperationStartedEventArgs(string fullPath)
        {
            FullPath = fullPath;
        }

        public string FullPath { get; }
    }
}