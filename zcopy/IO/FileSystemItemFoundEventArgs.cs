using System;
using System.IO;

namespace BananaHomie.ZCopy.IO
{
    public abstract class FileSystemItemFoundEventArgs<T> : EventArgs where T : FileSystemInfo
    {
        protected FileSystemItemFoundEventArgs(T item)
        {
            Item = item;
        }

        public T Item { get;}
    }
}