﻿using System;
using System.IO;

namespace BananaHomie.ZCopy.IO
{
    public class FileOperationProgressEventArgs
    {
        #region Properties

        public FileInfo SourceFile { get; }
        public FileInfo DestinationFile { get; }
        public long BytesCopied { get; }
        public long ChunkSize { get; }
        public double PercentComplete => (double) BytesCopied / SourceFile.Length * 100;

        #endregion

        #region Ctor

        public FileOperationProgressEventArgs(FileInfo sourceFile, FileInfo destinationFile, long bytesCopied, long chunkSize)
        {
            SourceFile = sourceFile;
            DestinationFile = destinationFile;
            BytesCopied = bytesCopied;
            ChunkSize = chunkSize;
        }

        #endregion

        #region Public methpds

        public TimeSpan GetEta(double seed, double speed)
        {
            return TimeSpan.FromSeconds(((double) SourceFile.Length - BytesCopied) / seed / speed);
        }

        #endregion
    }
}