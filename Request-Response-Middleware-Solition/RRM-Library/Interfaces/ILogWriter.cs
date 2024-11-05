using RRM_Library.Models;

namespace RRM_Library.Interfaces
{
    public interface ILogWriter
    {
        ILogMessageCreator MessageCreator { get; }
        Task WriteAsync(RequestResponseContext context);
    }
}
