using System.Text.RegularExpressions;

namespace BananaHomie.ZCopy.AnsiConsole
{
    public class AnsiConsole
    {
        public static string StripEscapeSequences(string value)
        {
            return Regex.Replace(value, "\\e\\[(\\d+;)*(\\d+)?[ABCDHJKfmsu]", "");
        }
    }
}