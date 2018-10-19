using BananaHomie.ZCopy.IO;
using BananaHomie.ZCopy.Logging;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace BananaHomie.ZCopy.Internal.Extensions
{
    internal static class ICopyProgressLoggerExtensions
    {
        public static void InitializeAll(this IEnumerable<ICopyProgressLogger> loggers, CommandLineApplication app, FileOperation fileOperation)
        {
            foreach (var logger in loggers)
                logger.Initialize(app, fileOperation);
        }

        public static void DeinitializeAll(this IEnumerable<ICopyProgressLogger> loggers, CommandLineApplication app, FileOperation fileOperation)
        {
            foreach (var logger in loggers)
                logger.Deinitialize(app, fileOperation);
        }
    }
}
