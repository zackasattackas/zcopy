using Microsoft.Win32.SafeHandles;

namespace BananaHomie.ZCopy.Internal
{
    internal class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeTokenHandle() 
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }
    }
}