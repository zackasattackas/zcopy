using BananaHomie.ZCopy.FileOperations;
using McMaster.Extensions.CommandLineUtils;

namespace BananaHomie.ZCopy.Logging
{
    internal interface ICopyProgressLogger
    {
        void Initialize(CommandLineApplication app, FileOperation operation);
        void Deinitialize(CommandLineApplication app, FileOperation operation);
    }
}
