namespace RRM_Library.LogWriters
{
    internal class NullLogWriter : ILogWriter
    {
        public ILogMessageCreator MessageCreator { get; }

        public Task WriteAsync(RequestResponseContext context)
        {
            return Task.CompletedTask;
        }
    }
}
