using System.IO;

namespace BananaHomie.ZCopy.Internal
{
    internal delegate void ProgressCallback(FileInfo source, FileInfo destination, long bytesCopied, long chunkSize);
}