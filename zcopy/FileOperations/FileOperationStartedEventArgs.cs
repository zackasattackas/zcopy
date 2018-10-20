namespace BananaHomie.ZCopy.FileOperations
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