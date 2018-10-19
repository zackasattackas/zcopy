using BananaHomie.ZCopy.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BananaHomie.ZCopy.IO
{
    public class FileMove : FileOperation
    {
        public MoveOptions Options { get; set; }        

        public FileMove(
            DirectoryInfo source,
            DirectoryInfo destination,
            List<ISearchFilter> fileFilters,
            List<ISearchFilter> directoryFilters,
            MoveOptions options)
            : base(source, destination, fileFilters, directoryFilters)
        {
            Options = options;
        }

        public void Move(CancellationToken cancellationToken = default)
        {
            Invoke(cancellationToken);
        }

        public override void Invoke(CancellationToken cancellationToken = default)
        {
            base.cancellation = cancellationToken;
            var searchOption = Options.HasFlag(MoveOptions.Recurse) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var search = new FileSystemSearch(Source, FileFilters, DirectoryFilters, searchOption);
            search.FileFound += SearchOnFileFound;

            search.Search(cancellationToken);
        }

        private void SearchOnFileFound(object sender, FileFoundEventArgs args)
        {
            var file = args.Item;
            var target = Utilities.GetDestinationFile(Source, file, Destination);

            try
            {
                Utilities.MoveFile(file, target, BufferSize, WhatToCopy, ProgressHandler, cancellation);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message + " " + e.InnerException?.Message);
            }

            Statistics.TotalFiles++;
            Statistics.BytesTransferred += file.Length;
        }

        internal override string GetOptionsString()
        {
            return Options.ToString();
        }
    }
}