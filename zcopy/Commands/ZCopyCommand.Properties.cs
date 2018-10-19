using BananaHomie.ZCopy.Logging;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BananaHomie.ZCopy.Commands
{
    internal partial class ZCopyCommand
    {
        #region Fields

        private static CancellationTokenSource cancellation;
        private List<ICopyProgressLogger> loggers;
        private static Stopwatch stopwatch;

        #endregion

        #region Properties (arguments / options)

        #region Arguments

        [Required]
        [Argument(0, "source", "The source directory. Use '.' for the current directory")]
        public string Source { get; set; }

        [Required]
        [Argument(1, "destination", "The destination directory. Use '.' for the current directory")]
        public string Destination { get; set; }

        [Argument(2, "files*", "The files to be copied")]
        public string FileFilter { get; set; } = "*.*";

        [Argument(3, "directories*", "The subdirectories to be copied if '--recurse' is specified")]
        public string DirectoriesFilter { get; set; } = "*";

        #endregion

        #region Impersonation parameters

        [Option("-u|--username", "Username to access the source and/or destination", CommandOptionType.SingleValue)]
        public string Username { get; set; }

        [Option("-p|--password", "Password to access the source and/or destination", CommandOptionType.SingleValue)]
        public string Password { get; set; }

        #endregion

        #region Copy options

        //[Option("-b|--buffer=<size>", "The buffer size to use. The default is 8192 bytes", CommandOptionType.SingleValue)]
        public int BufferSize { get; set; } = 8192;

        //[Option("-z|--restartable", "Restartable mode. This can negatively affect performance", CommandOptionType.NoValue)]
        //public bool Restartable { get; set; }

        [Option("-m|--move", "Delete the source files once copied", CommandOptionType.SingleValue)]
        public bool Move { get; set; }

        [Option("-r|--recurse", "Recusively scan the source directory for files to copy", CommandOptionType.NoValue)]
        public bool Recurse { get; set; }

        // ReSharper disable once InconsistentNaming
        [Option("-md5", "Compare the MD5 checksum of the copied file to the source", CommandOptionType.NoValue)]
        public bool VerifyMD5 { get; set; }

        [Option(
            "-mt|--multi-thread=<count>",
            "Multithreaded copy. The default thread count is 8",
            CommandOptionType.SingleOrNoValue)]
        public (bool HasValue, int? Value) ThreadCount { get; set; }

        [Option("-i|--info=<TAS>", "File info to copy. Allowed values are 'T=timestamps', 'A=attributes', 'S=security'",
            CommandOptionType.SingleValue)]
        public string WhatToCopy { get; set; } = "TAS";

        #endregion

        #region Logging options

        [LegalFilePath]
        //[Option("-lf|--log-file=<filePath>", "Write progress messages to a file", CommandOptionType.SingleValue)]
        public string LogFile { get; set; }

        //[Option("-le|--log-event", "Write progress messages to the Application event log", CommandOptionType.SingleValue)]
        //public bool EventLog { get; set; }

        #endregion

        #region Filtering options

        [Option("--regex", "Interpret search patterns as regular expressions*", CommandOptionType.NoValue)]
        public bool UseRegex { get; set; }

        [Option("-xf=<files*>", "Files to be excluded", CommandOptionType.SingleValue)]
        public string ExcludedFiles { get; set; }

        [Option("-xd=<directories*>", "Subdirectories to be excluded", CommandOptionType.SingleValue)]
        public string ExcludedDirectories { get; set; }

        [Option("-fa=<ACEHORS>", "File attributes to include/exclude. Append '!' to exclude the specified filters",
            CommandOptionType.SingleValue)]
        public string FileAttributeFilter { get; set; }

        [Option("-da=<ACEHORS>", "Directory attributes to include/exclude. Append '!' to exclude the specified filters",
            CommandOptionType.SingleValue)]
        public string DirectoryAttributeFilter { get; set; }

        [Option("--max-age=<datetime>", "Exclude files older than 'MM-dd-yyyy HH:mm'", CommandOptionType.SingleValue)]
        public string MaxFileAge { get; set; }

        [Option("--min-age=<datetime>", "Exclude files newer than 'MM-dd-yyyy HH:mm'", CommandOptionType.SingleValue)]
        public string MinFileAge { get; set; }

        [Option("--max-size=<bytes>", "Exclude files larger than n bytes", CommandOptionType.SingleValue)]
        public double MaxFileSize { get; set; }

        [Option("--min-size=<bytes>", "Exclude files smaller than n bytes", CommandOptionType.SingleValue)]
        public double MinFileSize { get; set; }

        #endregion

        #region Console output options

        [Option("-nc", "Do not write progress messages to the console", CommandOptionType.NoValue)]
        public bool NoConsoleOutput { get; set; }

        [Option("-cr=<ms>", "The interval in milliseconds to refresh progress information at the console", CommandOptionType.SingleValue)]
        public int RefreshInterval { get; set; } = 1000;

        [Option("-uom=<uom>", "Display copy speed in uom/s", CommandOptionType.SingleValue)]
        [AllowedValues("Kb", "KB", "KiB", "Mb", "MB", "MiB", "Gb", IgnoreCase = false)]
        public string CopySpeedUom { get; set; } = "Mb";

        [Option("-nh|--no-header", "Do not print the header information", CommandOptionType.NoValue)]
        public bool NoHeader { get; set; }

        [Option("-nf|--no-footer", "Do not print the footer information", CommandOptionType.NoValue)]
        public bool NoFooter { get; set; }

        [Option("-utc", "Interpret and display date/time values in coordinated universal time", CommandOptionType.NoValue)]        
        public bool UtcTime { get; set; }

        #endregion

        #endregion
    }
}
