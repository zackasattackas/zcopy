using System.IO;
using System.Threading;

namespace BananaHomie.ZCopy.FileOperations
{
    public interface IFileOperationHandler
    {
        WaitHandle WaitHandle { get; }
        void OnPreProcessing(FileInfo source, FileInfo target);
        void OnPostProcessing(FileInfo source, FileInfo target);
    }
}
