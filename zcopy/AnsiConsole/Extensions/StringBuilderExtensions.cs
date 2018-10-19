using System.Text;

namespace BananaHomie.ZCopy.AnsiConsole.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder SavePosition(this StringBuilder sb)
        {
            return sb.Append(EscapeCodes.SavePosition());
        }

        public static StringBuilder RestorePosition(this StringBuilder sb)
        {
            return sb.Append(EscapeCodes.RestorePosition());
        }

        public static StringBuilder Underline(this StringBuilder sb, string value)
        {
            return sb.Append(EscapeCodes.Underline).Append(value).Append(EscapeCodes.NoUnderline);
        }

        public static StringBuilder AppendColored(this StringBuilder sb, string value, string colorCode)
        {
            return sb.Append(colorCode).Append(value).Append(EscapeCodes.ForegroundDefault);
        }
    }
}
