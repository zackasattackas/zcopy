using BananaHomie.ZCopy.Commands;
using BananaHomie.ZCopy.Internal;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.Reflection;

namespace BananaHomie.ZCopy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<ZCopyCommand>(args);
            }
            catch (Exception e)
            {
                PrintError(e);
            }

#if DEBUG
            if (!Debugger.IsAttached)
                return;

            Console.Out.Write("\r\nPress any key to exit...");
            Console.ReadKey();
#endif
        }


        private static void PrintError(Exception e)
        {
            switch (e)
            {
                case AggregateException ae:
                    foreach (var ie in ae.InnerExceptions)
                        PrintException(ie);
                    return;
                case TargetInvocationException _:
                    e = e.InnerException;
                    break;
            }

            if (ZCopyConfiguration.Environment.DisableAnsiConsole)
                PrintException(e);
            else
                PrintExceptionAnsi(e);

            #region Local functions

            void PrintExceptionAnsi(Exception ex) =>
                ZCopyOutput.PrintError(ex.Message + " " + ex.InnerException?.Message);

            void PrintException(Exception ex) =>
                Utilities.Print(ex.Message + " " + ex.InnerException?.Message, color: ConsoleColor.Red);

            #endregion
        }
    }
}


