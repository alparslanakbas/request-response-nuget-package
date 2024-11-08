namespace RRM_Library.LogWriters
{
    internal class NullLogWriter : ILogWriter
    {
        public static readonly NullLogWriter Instance = new();

        public ILogMessageCreator? MessageCreator { get; } = null;

        private NullLogWriter() { }

        public Task WriteAsync(RequestResponseContext context)
        {
            return Task.CompletedTask;
        }
    }

}
