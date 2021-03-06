﻿using BananaHomie.ZCopy.AnsiConsole;
using BananaHomie.ZCopy.AnsiConsole.Extensions;
using System;
using Ansi = BananaHomie.ZCopy.AnsiConsole.AnsiConsole;

namespace BananaHomie.ZCopy.Internal
{
    internal class ZCopyOutput
    {
        public static void Print(string format, params object[] values)
        {
            Print(string.Format(format, values), false);
        }

        public static void Print(string value = "", bool newLine = true)
        {
            Console.Out.Write(Formalize(value, newLine));
        }

        public static void PrintError(string value, bool newLine = true)
        {
            Console.Error.Write(Formalize(value, newLine).ColorText(EscapeCodes.ForegroundRed));
        }

        private static string Formalize(string value, bool newLine)
        {
            if (DisableAnsiConsole())
                value = Ansi.StripEscapeSequences(value);

            if (newLine)
                value += "\r\n";

            return value;
        }

        private static bool DisableAnsiConsole()
        {
            return Console.IsOutputRedirected ||
                   ZCopyConfiguration.DisableAnsiConsole ||
                   (ZCopyConfiguration.Environment.DisableAnsiConsole ?? false);
        }
    }
}