using RRM_Library;
using RRM_Library.Interfaces;
using System.Text.Json;

namespace RRM_File_Logger.Library.MessageCreator
{
    internal class FileLoggerJsonMessageCreator : ILogMessageCreator
    {
        public string Create(RequestResponseContext context)
        {
            return $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss} - {JsonSerializer.Serialize(context)}";
        }
    }
}
