using BananaHomie.ZCopy.IO;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace BananaHomie.ZCopy.Internal
{
    internal static class Utilities
    {
        public static FileInfo GetDestinationFile(
            DirectoryInfo sourceDirectory,
            FileInfo sourceFile, 
            DirectoryInfo destinationDirectory)
        {
            var rel = sourceFile.DirectoryName?.Replace(sourceDirectory.FullName, "").TrimStart('\\') ?? string.Empty;            
            return new FileInfo(Path.Combine(destinationDirectory.FullName, rel , sourceFile.Name));
        }

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
                while ((read = fsread.Read(buffer, 0, bufferSize)) != 0 && !cancellationToken.IsCancellationRequested)
                {
                    fswrite.Write(buffer, 0, read);
                    callback?.Invoke(source, destination, copied += read, read);
                }
            }

            destination.Refresh();

            if (cancellationToken.IsCancellationRequested && destination.Length != source.Length)
            {
                destination.Delete();
                return;
            }
           
            if (whatToCopy.HasFlag(WhatToCopy.Timestamps))
            {
                destination.CreationTime = source.CreationTime;
                destination.LastAccessTime = source.LastAccessTime;
                destination.LastWriteTime = source.LastWriteTime;
            }
            if (whatToCopy.HasFlag(WhatToCopy.Attributes))
                destination.Attributes = source.Attributes;
            if (whatToCopy.HasFlag(WhatToCopy.Security))
                destination.SetAccessControl(source.GetAccessControl());
        }

        public static void Print(string value, bool newLine = true, ConsoleColor? color = null)
        {
            ConsoleColor current = default;
            if (color.HasValue)
            {
                current = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
            }

            if (newLine)
                value += Environment.NewLine;

            Console.Out.Write(value);

            if (color.HasValue)
                Console.ForegroundColor = current;
        }
    }
}
