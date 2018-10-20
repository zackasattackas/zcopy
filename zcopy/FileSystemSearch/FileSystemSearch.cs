using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BananaHomie.ZCopy.Internal.Extensions;
using JetBrains.Annotations;

namespace BananaHomie.ZCopy.FileSystemSearch
{
    public class FileSystemSearch
    {
        #region Properties

        public DirectoryInfo Source { get; }
        public List<ISearchFilter> FileFilters { get; }
        public List<ISearchFilter> DirectoryFilters { get; }
        public SearchOption SearchOption { get; }

        #endregion

        #region Events

        public event EventHandler<FileFoundEventArgs> FileFound;
        public event EventHandler<DirectoryFoundEventArgs> DirectoryFound;
        public event EventHandler<FileSystemSearchErrorEventArgs> Error;     

        #endregion

        #region Ctor

        public FileSystemSearch(
            [NotNull] DirectoryInfo source, 
            [NotNull] List<ISearchFilter> fileFilters, 
            [NotNull] List<ISearchFilter> directoryFilters, 
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Source = source;
            FileFilters = fileFilters;
            DirectoryFilters = directoryFilters;
            SearchOption = searchOption;
        }

        #endregion

        #region Public methods

        public void Search(CancellationToken cancellationToken = default)
        {
            SearchPrivate(this, cancellationToken);
        }

        #endregion

        #region Private methods

        private void SearchPrivate(FileSystemSearch searchCriteria, CancellationToken cancellationToken = default)
        {
            var filesFound = 0;
            foreach (var item in searchCriteria.Source.EnumerateFileSystemInfos())
                if (cancellationToken.IsCancellationRequested)
                    return;
                else if (item.IsDirectory())
                {
                    if (searchCriteria.SearchOption != SearchOption.AllDirectories ||
                        !item.MatchesAll(searchCriteria.DirectoryFilters))
                        continue;

                    OnDirectoryFound(this, new DirectoryFoundEventArgs((DirectoryInfo) item));

                    try
                    {
                        SearchPrivate(searchCriteria.Clone((DirectoryInfo) item), cancellationToken);
                    }
                    catch (Exception e)
                    {
                        OnError(this, new FileSystemSearchErrorEventArgs(item, e));
                    }
                }
                else if (item.MatchesAll(searchCriteria.FileFilters))
                {
                    OnFileFound(this, new FileFoundEventArgs((FileInfo) item));
                    filesFound++;
                }
        }

        private FileSystemSearch Clone(DirectoryInfo source)
        {
            return new FileSystemSearch(source,  FileFilters, DirectoryFilters, SearchOption);
        }

        #endregion

        #region Protected methods

        protected void OnFileFound(object sender, FileFoundEventArgs args)
        {
            FileFound?.Invoke(sender, args);
        }

        protected void OnDirectoryFound(object sender, DirectoryFoundEventArgs args)
        {
            DirectoryFound?.Invoke(sender, args);
        }

        protected void OnError(object sender, FileSystemSearchErrorEventArgs args)
        {
            Error?.Invoke(sender, args);
        }

        #endregion
    }
}
