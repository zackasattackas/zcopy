using System.Text;

namespace BananaHomie.ZCopy.AnsiConsole.Extensions
{
    public static class StringExtensions
    {
        public static string Underline(this string s)
        {
            return new StringBuilder(EscapeCodes.Underline).Append(s).Append(EscapeCodes.NoUnderline).ToString();
        }

        public static string ColorText(this string s, string colorCode)
        {
            return new StringBuilder(colorCode).Append(s).Append(EscapeCodes.ForegroundDefault).ToString();
        }
    }
}
