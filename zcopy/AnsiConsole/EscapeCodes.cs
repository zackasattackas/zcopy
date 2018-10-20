namespace BananaHomie.ZCopy.AnsiConsole
{
    public static class EscapeCodes
    {
        private static class AnsiForegroundColor
        {
            public const string Black = "30";
            public const string Red = "31";
            public const string Green = "32";
            public const string Yellow = "33";
            public const string Blue = "34";
            public const string Magenta = "35";
            public const string Cyan = "36";
            public const string White = "37";
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public const string Default = "39";
            public const string BrightBlack = "90";
            public const string BrightRed = "91";
            public const string BrightGreen = "92";
            public const string BrightYellow = "99";
            public const string BrightBlue = "94";
            public const string BrightMagenta = "95";
            public const string BrightCyan = "96";
            public const string BrightWhite = "97";
        }

        public const string Esc = "\u001b";
        public const string LeftSquareBracket = "\u005B";
        public const string RightSquareBracket = "\u005D";

        public const string Default = Esc + LeftSquareBracket + "0m";
        public const string BoldBright = Esc + LeftSquareBracket + "1m";
        public const string Underline = Esc + LeftSquareBracket + "4m";
        public const string NoUnderline = Esc + LeftSquareBracket + "24m";
        public const string ForegroundBlack = Esc + LeftSquareBracket + AnsiForegroundColor.Black + "m";
        public const string ForegroundRed = Esc + LeftSquareBracket + AnsiForegroundColor.Red + "m";
        public const string ForegroundGreen = Esc + LeftSquareBracket + AnsiForegroundColor.Green + "m";
        public const string ForegroundYellow = Esc + LeftSquareBracket + AnsiForegroundColor.Yellow + "m";
        public const string ForegroundBlue = Esc + LeftSquareBracket + AnsiForegroundColor.Blue + "m";
        public const string ForegroundMagenta = Esc + LeftSquareBracket + AnsiForegroundColor.Magenta + "m";
        public const string ForegroundCyan = Esc + LeftSquareBracket + AnsiForegroundColor.Cyan + "m";
        public const string ForegroundWhite = Esc + LeftSquareBracket + AnsiForegroundColor.White + "m";
        public const string ForegroundDefault = Esc + LeftSquareBracket + AnsiForegroundColor.Default + "m";
        public const string BrightForegroundBlack = Esc + LeftSquareBracket + AnsiForegroundColor.BrightBlack + "m";
        public const string BrightForegroundRed = Esc + LeftSquareBracket + AnsiForegroundColor.BrightRed + "m";
        public const string BrightForegroundGreen = Esc + LeftSquareBracket + AnsiForegroundColor.BrightGreen + "m";
        public const string BrightForegroundYellow = Esc + LeftSquareBracket + AnsiForegroundColor.BrightYellow + "m";
        public const string BrightForegroundBlue = Esc + LeftSquareBracket + AnsiForegroundColor.BrightBlue + "m";
        public const string BrightForegroundMagenta = Esc + LeftSquareBracket + AnsiForegroundColor.BrightMagenta + "m";
        public const string BrightForegroundCyan = Esc + LeftSquareBracket + AnsiForegroundColor.BrightCyan + "m";
        public const string BrightForegroundWhite = Esc + LeftSquareBracket + AnsiForegroundColor.BrightWhite + "m";

        public static string SavePosition()
        {
            return Esc + "7";
        }

        public static string RestorePosition()
        {
            return Esc + "8";
        }

        public static string EraseCharacters(int count = 1)
        {
            return Esc + LeftSquareBracket + count + "X";
        }
    }
}
