using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using BananaHomie.ZCopy.FileOperations;

namespace BananaHomie.ZCopy.Internal
{
    internal static class Impersonation
    {
        public static WindowsImpersonationContext ImpersonateUser(NetworkCredential credentials)
        {
            if (credentials == default)
                return default;

            if (!NativeMethods.LogonUser(
                credentials.UserName,
                credentials.Domain,
                credentials.Password,
                NativeMethods.LOGON32_LOGON_NEW_CREDENTIALS,
                NativeMethods.LOGON32_PROVIDER_WINNT50,
                out var token))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            using (token)
                return WindowsIdentity.Impersonate(token.DangerousGetHandle());
        }
    }
    internal static class FileUtils
    {
        public static FileInfo GetDestinationFile(
            DirectoryInfo sourceDirectory,
            FileInfo sourceFile, 
            DirectoryInfo destinationDirectory)
        {
            var rel = sourceFile.DirectoryName?.Replace(sourceDirectory.FullName, "").TrimStart('\\') ?? String.Empty;            
            return new FileInfo(Path.Combine(destinationDirectory.FullName, rel , sourceFile.Name));
        }

        public static void MoveFile(
            FileInfo source, 
            FileInfo destination, 
            int bufferSize = 4096,
            WhatToCopy whatToCopy = WhatToCopy.Data,
            ProgressCallback callback = default,
            CancellationToken cancellationToken = default)
        {
            CopyFile(source, destination, bufferSize, whatToCopy,callback, cancellationToken);

            if (destination.Exists && destination.Length == source.Length)
                source.Delete();
            else
                throw new FileCopyException("Failed to move file. The file was not found or the size does not match the source.", source.FullName);
        }

        public static void CopyFile(
            FileInfo source,
            FileInfo destination,
            int bufferSize = 4096,
            WhatToCopy whatToCopy = WhatToCopy.Data,
            ProgressCallback callback = default,
            CancellationToken cancellationToken = default)
        {
            if (!source.Exists)
                throw new FileNotFoundException("The source file was not found.", source.FullName);

            if (destination.Directory != null && !destination.Directory.Exists)
                destination.Directory.Create();

            var buffer = new byte[bufferSize];

            using (var fsread = new FileStream(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
            using (var fswrite = new FileStream(destination.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize))
            {
                int read;
                long copied = 0;
                while ((read = fsread.Read(buffer, 0, bufferSize)) != 0)
                {
                    fswrite.Write(buffer, 0, read);
                    callback?.Invoke(source, destination, copied += read, read);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                destination.Delete();
                cancellationToken.ThrowIfCancellationRequested();
            }

            SetFileInfo(source, destination, whatToCopy);       
            IntegrityCheck(source, destination, whatToCopy);
        }

        public static bool Equals(FileInfo x, FileInfo y, WhatToCopy whatToCompare)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            if (ReferenceEquals(x, y))
                return true;
            if (x.Length != y.Length)
                return false;
            if (x.Name != y.Name)
                return false;
            if (x.Extension != y.Extension)
                return false;
            if (whatToCompare.HasFlag(WhatToCopy.Attributes))
            {
                if (x.Attributes != y.Attributes)
                    return false;
            }
            if (whatToCompare.HasFlag(WhatToCopy.Timestamps))
            {
                if (x.CreationTime != y.CreationTime)
                    return false;
                if (x.LastWriteTime != y.LastWriteTime)
                    return false;
            }
            //if (whatToCompare.HasFlag(WhatToCopy.Security))
            //{
            //    var sourceAcl = x.GetAccessControl().GetSecurityDescriptorBinaryForm();
            //    var targetAcl = y.GetAccessControl().GetSecurityDescriptorBinaryForm();

            //    for (var i = 0; i < sourceAcl.Length; i++)
            //    {
            //        if (sourceAcl[i] != targetAcl[i])
            //            return false;
            //    }
            //}

            return true;
        }

        private static void SetFileInfo(FileInfo source, FileInfo destination, WhatToCopy whatToCopy)
        {
            if (whatToCopy == WhatToCopy.None || whatToCopy == WhatToCopy.Data)
                return;
            if (whatToCopy.HasFlag(WhatToCopy.Timestamps))
            {
                destination.CreationTime = source.CreationTime;
                destination.LastAccessTime = source.LastAccessTime;
                destination.LastWriteTime = source.LastWriteTime;
            }
            if (whatToCopy.HasFlag(WhatToCopy.Attributes))
                destination.Attributes = source.Attributes;
            if (whatToCopy.HasFlag(WhatToCopy.Security))
            {
                var fs = source.GetAccessControl();
                fs.SetAccessRuleProtection(true, true);
                destination.SetAccessControl(fs);
            }
        }

        private static void IntegrityCheck(FileInfo source, FileInfo destination, WhatToCopy whatToCompare)
        {
            destination.Refresh();

            if (!Equals(source, destination, whatToCompare))
                throw new ZCopyException($"The file {destination.FullName} failed an integrity check.");
        }
    }
}
