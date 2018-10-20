using BananaHomie.ZCopy.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;

namespace BananaHomie.ZCopy.FileOperations
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FileHash
    {
        #region Fields

        private Func<FileInfo, HashAlgorithm, byte[]> asyncDelegate;

        #endregion

        #region Public methods

        public IAsyncResult BeginComputeMD5Hash(FileInfo file, AsyncCallback callback, object state)
        {
            return BeginComputeHash(file, MD5.Create(), callback, state);
        }

        public string EndComputeMD5Hash(IAsyncResult asyncResult)
        {
            return EndComputeHash(asyncResult).ToHexString();
        }

        public IAsyncResult BeginComputeHash(
            FileInfo file,
            HashAlgorithm algorithm,
            AsyncCallback callback,
            object state)
        {
            asyncDelegate = ComputeHash;

            return asyncDelegate.BeginInvoke(file, algorithm, callback, state);
        }

        public IEnumerable<byte> EndComputeHash(IAsyncResult asyncResult)
        {
            return asyncDelegate.EndInvoke(asyncResult);
        }

        public static byte[] ComputeHash(FileInfo file, HashAlgorithm algorithm)
        {
            using (algorithm)
            using (var fstream = file.OpenRead())
                return algorithm.ComputeHash(fstream);
        }

        #endregion
    }
}