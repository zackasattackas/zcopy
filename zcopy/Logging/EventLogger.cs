using BananaHomie.ZCopy.FileOperations;
using McMaster.Extensions.CommandLineUtils;

namespace BananaHomie.ZCopy.Logging
{
    internal class EventLogger : ICopyProgressLogger
    {

        public void Initialize(CommandLineApplication app, FileOperation operation)
        {
            throw new System.NotImplementedException();
        }

        public void Deinitialize(CommandLineApplication app, FileOperation operation)
        {
            throw new System.NotImplementedException();
        }
    }
}