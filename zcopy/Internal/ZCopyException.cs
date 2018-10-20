using System;

namespace BananaHomie.ZCopy.Internal
{
    internal class ZCopyException : Exception
    {
        public ZCopyException(string message, Exception innerException = default)
            :base(message, innerException)
        {            
        }
    }
}
