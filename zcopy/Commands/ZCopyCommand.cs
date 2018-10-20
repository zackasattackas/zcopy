using BananaHomie.ZCopy.Internal;
using BananaHomie.ZCopy.Internal.Extensions;
using BananaHomie.ZCopy.Logging;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace BananaHomie.ZCopy.Commands
{
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [Command(
        name: "zcopy",
        Description = "File copy with progress!",
        FullName = "zcopy",
        ExtendedHelpText =
            "\r\n*Search filters support wildcards by default. Use '--regex' to enable regular expression pattern matching")]
    internal partial class ZCopyCommand
    {
        public ZCopyCommand()
        {
            cancellation = new CancellationTokenSource();
            loggers = new List<ICopyProgressLogger>();
        }

        [UsedImplicitly]
        protected void OnExecute(CommandLineApplication app)
        {
            SetConsoleSettings();
            ValidateArguments();
            HookCancelKeyPressEvent();

            ZCopyConfiguration.UseUtc = UtcTime;
            ZCopyConfiguration.RefreshInterval = TimeSpan.FromMilliseconds(RefreshInterval);
            if (DisableAnsiConsole)
                ZCopyConfiguration.Environment.DisableAnsiConsole = DisableAnsiConsole;

            stopwatch = Stopwatch.StartNew();
            var operation = Move ? NewFileMoveOperation() : NewFileCopyOperation();

            if (!NoHeader)
                PrintHeader(app, operation);

            loggers = GetLoggers();
            loggers.InitializeAll(app, operation);
            operation.Invoke(cancellation.Token);
            loggers.DeinitializeAll(app, operation);
            stopwatch.Stop();

            if (!NoFooter)
                PrintFooter(operation);

            ResetConsoleSettings();
        }

        private static void SetConsoleSettings()
        {
            Console.CursorVisible = false;

            if (Console.BufferWidth < 120)
                Console.BufferWidth = 120;
            if (ZCopyConfiguration.Environment.DisableAnsiConsole)
                return;
            if (!NativeMethods.GetConsoleMode(NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE), out var mode))
                Helpers.ThrowLastWin32Exception();
            if (!NativeMethods.SetConsoleMode(NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE), (mode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING) ^ NativeMethods.ENABLE_WRAP_AT_EOL_OUTPUT))
                Helpers.ThrowLastWin32Exception();
        }

        private static void ResetConsoleSettings()
        {
            Console.CursorVisible = true;

            if (ZCopyConfiguration.Environment.DisableAnsiConsole)
                return;
            if (!NativeMethods.GetConsoleMode(NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE), out var mode))
                Helpers.ThrowLastWin32Exception();
            if (!NativeMethods.SetConsoleMode(NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE), (mode ^ NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING) | NativeMethods.ENABLE_WRAP_AT_EOL_OUTPUT))
                Helpers.ThrowLastWin32Exception();
        }
    }
}
