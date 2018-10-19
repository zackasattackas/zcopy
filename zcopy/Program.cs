using BananaHomie.ZCopy.Commands;
using BananaHomie.ZCopy.Internal;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.Reflection;
using static BananaHomie.ZCopy.Internal.NativeMethods;

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

            PrintException(e);            

            void PrintException(Exception ex) => Utilities.Print(ex.Message + " " + ex.InnerException?.Message, color: ConsoleColor.Red);
        }
    }
}
