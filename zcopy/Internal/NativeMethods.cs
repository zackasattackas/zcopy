using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace BananaHomie.ZCopy.Internal
{
    internal class NativeMethods
    {
        #region Constants

        public const uint LOGON32_LOGON_NEW_CREDENTIALS = 9;
        public const uint LOGON32_PROVIDER_WINNT50 = 3;
        public const int STD_OUTPUT_HANDLE = -11;
        public const int ENABLE_WRAP_AT_EOL_OUTPUT = 0x2;
        public const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x4;
        public const int ATTACH_PARENT_PROCESS = -1;

        #endregion

        #region Extern methods

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool LogonUser(
            string lpszUsername, 
            string lpszDomain, 
            string lpszPasswordm,
            uint dwLogonType, 
            uint dwLogonProvider, 
            out SafeTokenHandle phToken);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32", SetLastError = true)]
        public static extern SafeFileHandle GetStdHandle(int nStdHandle);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool GetConsoleMode([In] SafeFileHandle hConsoleMode, [Out] out int dwMode);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool SetConsoleMode([In] SafeFileHandle hConsoleMode, [In] int dwMode);

        #endregion
    }
}
