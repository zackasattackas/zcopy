using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BananaHomie.ZCopy.FileSystemSearch;
using BananaHomie.ZCopy.Internal;

namespace BananaHomie.ZCopy.FileOperations
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

        public override void Invoke(CancellationToken cancellationToken = default)
        {
            base.cancellation = cancellationToken;
            var searchOption = Options.HasFlag(MoveOptions.Recurse) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var search = new FileSystemSearch.FileSystemSearch(Source, FileFilters, DirectoryFilters, searchOption);
            search.FileFound += SearchOnFileFound;
            search.Error += SearchOnError;

            search.Search(cancellationToken);
        }

        private void SearchOnFileFound(object sender, FileFoundEventArgs args)
        {
            var file = args.Item;
            var target = FileUtils.GetDestinationFile(Source, file, Destination);

            try
            {
                cancellation.ThrowIfCancellationRequested();
                TryMoveFile(file, target);
                target.Refresh();
                OnCompleted(this, new FileOperationCompletedEventArgs(target));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                OnError(this, new FileOperationErrorEventArgs(e));
            }
        }

        private void SearchOnError(object sender, FileSystemSearchErrorEventArgs e)
        {
            switch (e.Item)
            {
                case FileInfo _:
                    Statistics.SkippedFiles++;
                    break;
                case DirectoryInfo _:
                    Statistics.SkippedDirectories++;
                    break;
            }
        }

        internal override string GetOptionsString()
        {
            return Options.ToString();
        }
    }
}