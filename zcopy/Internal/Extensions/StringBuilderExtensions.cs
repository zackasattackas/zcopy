using System.Text;

namespace BananaHomie.ZCopy.Internal.Extensions
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendIf(this StringBuilder sb, bool expression, string value)
        {
            if (expression)
                sb.Append(value);

            return sb;
        }
    }
}
