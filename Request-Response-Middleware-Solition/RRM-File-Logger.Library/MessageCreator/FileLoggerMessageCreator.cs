using RRM_Library;
using RRM_Library.Interfaces;

namespace RRM_File_Logger.Library.MessageCreator
{
    internal class FileLoggerMessageCreator : ILogMessageCreator
    {
        public string Create(RequestResponseContext context)
        {
            string message =
                $"datetime: {DateTime.Now:yyyy-MM-dd_HH-mm-ss} - " +
                $"[{context.FormatedRequestTime}] " +
                $"[{context.RequestTime}] " +
                $"[{context.Url.PathAndQuery}] " +
                $"[{context.Request}] " +
                $"[{context.Response}] " +
                $"[{context.StatusCode}] " +
                $"[{context.ClientIPAddress}] " +
                $"[{context.ExternalIPAddress}] " +
                $"[{context.UserAgent}] " +
                $"[{context.Cookies}] " +
                $"[{context.Method}]";
            return message;
        }
    }
}
