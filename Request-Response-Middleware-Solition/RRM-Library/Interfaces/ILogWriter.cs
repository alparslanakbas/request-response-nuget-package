using RRM_Library.Models;

namespace RRM_Library.Interfaces
{
    public interface ILogWriter
    {
        public ILogMessageCreator MessageCreator { get; }
        Task WriteAsync(RequestResponseContext context);
    }
}
