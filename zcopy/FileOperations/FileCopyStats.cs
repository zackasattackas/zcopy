namespace BananaHomie.ZCopy.FileOperations
{
    public class FileCopyStats
    {
        public int TotalFiles { get; set; }
        public long BytesTransferred { get; set; }
        public int SkippedFiles { get; set; }
        public int SkippedDirectories { get; set; }
        public int TotalRetries { get; set; }
    }
}