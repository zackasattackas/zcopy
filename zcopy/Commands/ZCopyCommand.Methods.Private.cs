using BananaHomie.ZCopy.AnsiConsole;
using BananaHomie.ZCopy.AnsiConsole.Extensions;
using BananaHomie.ZCopy.FileOperations;
using BananaHomie.ZCopy.FileSystemSearch;
using BananaHomie.ZCopy.Internal;
using BananaHomie.ZCopy.Internal.Extensions;
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
            ZCopyOutput.Print($"\r\n{GetVersion()}\r\n");
            ZCopyOutput.Print($"{zcopyMetadata.Name} - {zcopyMetadata.Description}\r\n");
            ZCopyOutput.Print($"Started {(ZCopyConfiguration.UseUtc ? DateTime.UtcNow : DateTime.Now).ToString("dddd, MMM dd yyyy hh:mm tt" + (ZCopyConfiguration.UseUtc ? "" : " (zzzz)"))}\r\n");

            const string fmt = " {0} {1}\r\n";

            ZCopyOutput.Print(fmt, FormatHeader("Source"), operation.Source.FullName);
            ZCopyOutput.Print(fmt, FormatHeader("Destination"), operation.Destination.FullName);
            ZCopyOutput.Print(fmt, FormatHeader("Files"), app.Arguments[2].Values.Any() ? string.Join(", ",  app.Arguments[2].Values) : "*.*");
            ZCopyOutput.Print(fmt, FormatHeader("Options"), GetUserOptions());
            ZCopyOutput.Print();

            #region Local functions

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

            #endregion
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

            ZCopyOutput.Print($"{elapsed.ToString(elapsedFmt)} elapsed | " +
                                  $"{stats.TotalFiles} file(s) ({transferred}) {(operation is FileMove ? "moved" : "copied")} | " +
                                  $"{Helpers.CopySpeedToString(uom, speed)} (avg)");
            ZCopyOutput.Print(cancellation.IsCancellationRequested? "\r\nThe operation was cancelled" : "The operation completed successfully");
        }

        private List<ICopyProgressLogger> GetLoggers()
        {
            var list = new List<ICopyProgressLogger>();
            ZCopyConfiguration.CopySpeedUom = GetCopySpeedUom();

            if (LogFile != null && Console.IsOutputRedirected)
                throw new ArgumentException("Cannot write to a log file when output is redirected.");

            if (Console.IsOutputRedirected)
                list.Add(new FileLogger());
            else
            {
                if (LogFile != null)
                    list.Add(new FileLogger(LogFile, false));                
                if (NoConsoleOutput || LogFile != null && !TeeOutput) // If '-lf' is specified but not '--tee', a console logger will not be used
                    return list;
                if (BasicConsoleOutput || ThreadCount.HasValue)
                    list.Add(new BasicConsoleLogger());
                else
                    list.Add(new ConsoleLogger());
            }

            return list;
        }

        private FileOperation NewFileCopyOperation()
        {
            var fileCopy = new FileCopy(
                new DirectoryInfo(Source),
                new DirectoryInfo(Destination),
                GetFileFilters(),
                GetDirectoryFilters(),
                GetOptions())
            {
                BufferSize = BufferSize, Credentials = GetCredentials(), WhatToCopy = GetWhatToCopy(),
                RetryCount = RetryCount, RetryInterval = TimeSpan.FromSeconds(RetryIntervalSeconds)
            };

            return ThreadCount.HasValue ? (FileOperation) fileCopy.MakeMultiThreaded(ThreadCount.Value ?? 8) : fileCopy;
        }

        private FileOperation NewFileMoveOperation()
        {
            var fileMove = new FileMove(
                new DirectoryInfo(Source),
                new DirectoryInfo(Destination),
                GetFileFilters(),
                GetDirectoryFilters(),
                GetOptions())
            {
                BufferSize = BufferSize, Credentials = GetCredentials(), WhatToCopy = GetWhatToCopy(),
                RetryCount = RetryCount, RetryInterval = TimeSpan.FromSeconds(RetryIntervalSeconds)
            };

            return ThreadCount.HasValue ? (FileOperation) fileMove.MakeMultiThreaded(ThreadCount.Value ?? 8) : fileMove;
        }

        private FileOperationOptions GetOptions()
        {
            var flags = FileOperationOptions.None;

            if (Recurse)
                flags |= FileOperationOptions.Recurse;

            if (VerifyMD5)
                flags |= FileOperationOptions.VerifyMD5;

            return flags;
        }

        private WhatToCopy GetWhatToCopy()
        {
            var flags = FileOperations.WhatToCopy.Data;

            if (WhatToCopy == null)
                return flags;

            foreach (var c in WhatToCopy.ToCharArray())
                switch (c)
                {
                    case 'T':
                        flags |= FileOperations.WhatToCopy.Timestamps;
                        break;
                    case 'A':
                        flags |= FileOperations.WhatToCopy.Attributes;
                        break;
                    case 'S':
                        flags |= FileOperations.WhatToCopy.Security;
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
                filters.Add(FileSystemSearch.FileAttributeFilter.Parse(FileAttributeFilter.ToCharArray()));
            if (MaxFileAge != null)
                filters.Add(new FileAgeFilter(MaxFileAge, UtcTime, true));
            if (MinFileAge != null)
                filters.Add(new FileAgeFilter(MinFileAge, UtcTime, false));
            if (MaxFileSize > 0)
                filters.Add(new MaxFileSizeFilter(MaxFileSize));
            if (MinFileSize > 0)
                filters.Add(new MinFileSizeFilter(MinFileSize));
            if (!IncludeSystem && !filters.Any(f => f is FileAttributeFilter af && (af.Attributes & FileAttributes.System) > 0 && af.Exclude))
                filters.Add(new FileAttributeFilter(FileAttributes.System, true));

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
                filters.Add(FileSystemSearch.FileAttributeFilter.Parse(DirectoryAttributeFilter.ToCharArray()));
            if (!IncludeSystem && !filters.Any(f => f is FileAttributeFilter af && (af.Attributes & FileAttributes.System) > 0 && af.Exclude))
                filters.Add(new FileAttributeFilter(FileAttributes.System, true));
            return filters;
        }

        private NetworkCredential GetCredentials()
        {
            if (Username == default)
                return default;
            var domainUser = Username.Split('@', '\\');
            return new NetworkCredential(
                domainUser[0], 
                domainUser.Length > 1 ? domainUser[1] : default,
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
