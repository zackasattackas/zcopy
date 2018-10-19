using BananaHomie.ZCopy.AnsiConsole;
using BananaHomie.ZCopy.AnsiConsole.Extensions;
using BananaHomie.ZCopy.Internal;
using BananaHomie.ZCopy.Internal.Extensions;
using BananaHomie.ZCopy.IO;
using BananaHomie.ZCopy.Logging;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace BananaHomie.ZCopy.Commands
{
    internal partial class ZCopyCommand
    {
        private void ValidateArguments()
        {
            if (Source == Destination)
                throw new ArgumentException("The source and destination directories cannot match.");

            Source = Path.GetFullPath(Source);
            Destination = Path.GetFullPath(Destination);

            if (Username != null)
                return;

            if (!Directory.Exists(Source))
                throw new DirectoryNotFoundException("The source directory was not found.");

            if (!Directory.Exists(Destination))
                Directory.CreateDirectory(Destination);
        }

        private static void HookCancelKeyPressEvent()
        {
            Console.CancelKeyPress += (sender, a) =>
            {
                a.Cancel = true;
                cancellation.Cancel();
            };
        }

        public static void PrintHeader(CommandLineApplication app, FileOperation operation)
        {
            var zcopyMetadata = typeof(ZCopyCommand).GetCustomAttribute<CommandAttribute>();
            Console.Out.WriteLine($"\r\n{GetVersion()}\r\n");
            Console.Out.WriteLine($"{zcopyMetadata.Name} - {zcopyMetadata.Description}\r\n");
            Console.Out.WriteLine($"Started {(ZCopyConfiguration.UseUtc ? DateTime.UtcNow : DateTime.Now).ToString("dddd, MMM dd yyyy hh:mm tt" + (ZCopyConfiguration.UseUtc ? "" : " (zzzz)"))}\r\n");

            const string fmt = " {0} {1}";

            Console.Out.WriteLine(fmt, FormatHeader("Source"), operation.Source.FullName);
            Console.Out.WriteLine(fmt, FormatHeader("Destination"), operation.Destination.FullName);
            Console.Out.WriteLine(fmt, FormatHeader("Files"), String.Join(", ", app.Arguments[2].Values));
            Console.Out.WriteLine(fmt, FormatHeader("Options"), GetUserOptions());
            Console.Out.WriteLine();

            string FormatHeader(string value)
            {
                return value.PadLeft(12).ColorText(EscapeCodes.ForegroundCyan);
            }
            string GetUserOptions()
            {
                var args = Environment.GetCommandLineArgs();
                var userOptBldr = new StringBuilder();

                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i][0] != '-' && i <= 3)
                        continue;

                    userOptBldr.Append(args[i] + " ");
                }

                return userOptBldr.ToString();
            }
        }

        public static void PrintFooter(FileOperation operation)
        {
            Console.SetCursorPosition(0, Console.CursorTop + 2);         

            var stats = operation.Statistics;
            var transferred = new FileSize(stats.BytesTransferred);
            var elapsed = stopwatch.Elapsed;
            var elapsedFmt = new StringBuilder()
                .AppendIf(elapsed.TotalDays > 1, "dd\\d")
                .AppendIf(elapsed.TotalHours > 1, "hh\\h")
                .AppendIf(elapsed.TotalMinutes > 1, "mm\\m")
                .Append("ss\\.fff\\s").ToString();

            var (speedbase, uom) = Helpers.GetCopySpeedBase(ZCopyConfiguration.CopySpeedUom);
            var speed = Helpers.GetCopySpeed(stats.BytesTransferred, speedbase, elapsed);

            Console.Out.WriteLine($"{elapsed.ToString(elapsedFmt)} elapsed | " +
                                  $"{stats.TotalFiles} file(s) ({transferred}) {(operation is FileMove ? "moved" : "copied")} | " +
                                  $"{Helpers.CopySpeedToString(uom, speed)} (avg)");
            Console.Out.WriteLine(cancellation.IsCancellationRequested? "\r\nThe operation was cancelled" : "The operation completed successfully");
        }

        private List<ICopyProgressLogger> GetLoggers()
        {
            var list = new List<ICopyProgressLogger>();
            var uom = GetCopySpeedUom();

            if (!NoConsoleOutput && !Console.IsOutputRedirected)
                if (ThreadCount.HasValue)
                    list.Add(new MultiThreadedConsoleLogger(uom));
                else
                    list.Add(new ConsoleLogger(uom));
            if (LogFile != null)
                list.Add(new FileLogger(LogFile, false));

            ZCopyConfiguration.CopySpeedUom = uom;

            return list;
        }

        private FileOperation NewFileCopyOperation()
        {
            var fileCopy = new FileCopy(
                    new DirectoryInfo(Source),
                    new DirectoryInfo(Destination),
                    GetFileFilters(),
                    GetDirectoryFilters(),
                    GetCopyOptions())
            { BufferSize = BufferSize, Credentials = GetCredentials(), WhatToCopy = GetWhatToCopy() };

            return ThreadCount.HasValue ? (FileOperation) fileCopy.MakeMultiThreaded(ThreadCount.Value ?? 8) : fileCopy;

            CopyOptions GetCopyOptions()
            {
                var flags = CopyOptions.None;

                if (Recurse)
                    flags |= CopyOptions.Recurse;

                if (VerifyMD5)
                    flags |= CopyOptions.VerifyMD5;

                return flags;
            }
        }

        private FileMove NewFileMoveOperation()
        {
            return new FileMove(
                    new DirectoryInfo(Source),
                    new DirectoryInfo(Destination),
                    GetFileFilters(),
                    GetDirectoryFilters(),
                    GetMoveOptions())
            { BufferSize = BufferSize, Credentials = GetCredentials(), WhatToCopy = GetWhatToCopy() };

            MoveOptions GetMoveOptions()
            {
                var flags = MoveOptions.None;

                if (Recurse)
                    flags |= MoveOptions.Recurse;

                if (VerifyMD5)
                    flags |= MoveOptions.VerifyMD5;

                return flags;
            }
        }

        private WhatToCopy GetWhatToCopy()
        {
            var flags = IO.WhatToCopy.Data;

            if (WhatToCopy == null)
                return flags;

            foreach (var c in WhatToCopy.ToCharArray())
                switch (c)
                {
                    case 'T':
                        flags |= IO.WhatToCopy.Timestamps;
                        break;
                    case 'A':
                        flags |= IO.WhatToCopy.Attributes;
                        break;
                    case 'S':
                        flags |= IO.WhatToCopy.Security;
                        break;
                    default:
                        throw new ArgumentException(
                            $"Invalid option value: {WhatToCopy}.\r\nAllowed values are 'T=timestamps', 'A=attributes', 'S=security'");
                }

            return flags;
        }

        private List<ISearchFilter> GetFileFilters()
        {
            var filters = new List<ISearchFilter>();

            if (FileFilter != null)
                filters.Add(new FileNameFilter(UseRegex, ParseFilters(FileFilter)));
            if (ExcludedFiles != null)
                filters.Add(new FileNameFilter(UseRegex, ParseFilters(ExcludedFiles), true));
            if (FileAttributeFilter != null)
                filters.Add(new FileAttributeFilter(FileAttributeFilter.ToCharArray()));
            if (MaxFileAge != null)
                filters.Add(new FileAgeFilter(MaxFileAge, UtcTime, true));
            if (MinFileAge != null)
                filters.Add(new FileAgeFilter(MinFileAge, UtcTime, false));
            if (MaxFileSize > 0)
                filters.Add(new MaxFileSizeFilter(MaxFileSize));
            if (MinFileSize > 0)
                filters.Add(new MinFileSizeFilter(MinFileSize));

            return filters;
        }

        private List<ISearchFilter> GetDirectoryFilters()
        {
            var filters = new List<ISearchFilter>();

            // If the destination is nested inside the source, we need to exlude the destination
            // directory from searches
            if (Destination.StartsWith(Source, StringComparison.CurrentCultureIgnoreCase))
                filters.Add(dir => !dir.FullName.StartsWith(Destination, StringComparison.CurrentCultureIgnoreCase));
            if (DirectoriesFilter != null)
                filters.Add(new FileNameFilter(UseRegex, ParseFilters(DirectoriesFilter)));
            if (ExcludedDirectories != null)
                filters.Add(new FileNameFilter(UseRegex, ParseFilters(ExcludedDirectories), true));
            if (DirectoryAttributeFilter != null)
                filters.Add(new FileAttributeFilter(DirectoryAttributeFilter.ToCharArray()));

            return filters;
        }

        private NetworkCredential GetCredentials()
        {
            if (Username == default)
                return default;
            var domainUser = Username.Split('@', '\\');
            return new NetworkCredential(domainUser[0], domainUser.Length > 1 ? domainUser[1] : default,
                Password ?? Prompt.GetPassword("Password: "));
        }

        private static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static IEnumerable<string> ParseFilters(string value)
        {
            return value.Split(',').Select(f => f.Trim());
        }

        private CopySpeedUomTypes GetCopySpeedUom()
        {
            switch (CopySpeedUom)
            {
                case "B":
                    return CopySpeedUomTypes.Bytes;
                case "b":
                    return CopySpeedUomTypes.Bits;
                case "Kb":
                    return CopySpeedUomTypes.Kilobits;
                case "KB":
                    return CopySpeedUomTypes.Kilobytes;
                case "KiB":
                    return CopySpeedUomTypes.Kibibytes;
                case "Mb":
                    return CopySpeedUomTypes.Megabits;
                case "MB":
                    return CopySpeedUomTypes.Megabytes;
                case "MiB":
                    return CopySpeedUomTypes.Mebibytes;
                case "Gb":
                    return CopySpeedUomTypes.Gigabits;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
