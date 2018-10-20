using System.IO;

namespace BananaHomie.ZCopy.FileOperations
{
    internal delegate void ProgressCallback(FileInfo source, FileInfo destination, long bytesCopied, long chunkSize);
}