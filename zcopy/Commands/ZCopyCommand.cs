using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using BananaHomie.ZCopy.Internal;
using BananaHomie.ZCopy.Internal.Extensions;
using BananaHomie.ZCopy.Logging;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using static BananaHomie.ZCopy.Internal.NativeMethods;

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
            operation.Start(cancellation.Token);
            loggers.DeinitializeAll(app, operation);
            stopwatch.Stop();

            if (!NoFooter)
                PrintFooter(operation);

            ResetConsoleSettings();
        }

        private static void SetConsoleSettings()
        {
            if (Console.IsOutputRedirected)
                return;
            //Console.CursorVisible = false;

            if (Console.BufferWidth < 135)
                Console.BufferWidth = 135;

            if (ZCopyConfiguration.Environment.DisableAnsiConsole)
                return;
            if (!GetConsoleMode(GetStdHandle(STD_OUTPUT_HANDLE), out var mode))
                Helpers.ThrowLastWin32Exception();
            if (!SetConsoleMode(GetStdHandle(STD_OUTPUT_HANDLE), (mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING) ^ ENABLE_WRAP_AT_EOL_OUTPUT))
                Helpers.ThrowLastWin32Exception();

        }

        private static void ResetConsoleSettings()
        {
            if (Console.IsOutputRedirected)
                return;

            //Console.CursorVisible = true;

            if (ZCopyConfiguration.Environment.DisableAnsiConsole)
                return;
            if (!GetConsoleMode(GetStdHandle(STD_OUTPUT_HANDLE), out var mode))
                Helpers.ThrowLastWin32Exception();
            if (!SetConsoleMode(GetStdHandle(STD_OUTPUT_HANDLE), (mode ^ ENABLE_VIRTUAL_TERMINAL_PROCESSING) | ENABLE_WRAP_AT_EOL_OUTPUT))
                Helpers.ThrowLastWin32Exception();
        }
    }
}
