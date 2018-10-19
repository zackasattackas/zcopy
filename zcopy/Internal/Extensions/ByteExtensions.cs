using System.Collections.Generic;
using System.Text;

namespace BananaHomie.ZCopy.Internal.Extensions
{
    internal static class ByteExtensions
    {
        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            var bldr = new StringBuilder();

            foreach (var b in bytes)
                bldr.AppendFormat("{0:x2}", b);

            return bldr.ToString();
        }
    }
}
