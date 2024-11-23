using RRM_File_Logger.Library.MessageCreator;
using RRM_File_Logger.Library.Models;
using RRM_Library;
using RRM_Library.Interfaces;

namespace RRM_File_Logger.Library.LogWriters
{
    internal class FileLogWriter : ILogWriter
    {
        readonly FileLoggingOptions _fileLoggingOptions;

        internal FileLogWriter(FileLoggingOptions fileLoggingOptions)
        {
            _fileLoggingOptions = fileLoggingOptions;
            MessageCreator = fileLoggingOptions.UseJsonFormat ? new FileLoggerJsonMessageCreator() : new FileLoggerMessageCreator();

            fileLoggingOptions.ValidatePath();
        }

        public ILogMessageCreator? MessageCreator { get; }

        public async Task WriteAsync(RequestResponseContext context)
        {
            var message = MessageCreator!.Create(context);

            var fullPathName = _fileLoggingOptions.GetFullFilePath();

            await File.AppendAllTextAsync(fullPathName, message + Environment.NewLine);
        }
    }
}
