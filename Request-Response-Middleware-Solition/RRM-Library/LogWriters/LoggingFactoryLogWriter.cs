namespace RRM_Library.LogWriters
{
    internal class LoggingFactoryLogWriter : ILogWriter
    {
        readonly ILoggerFactory _loggerFactory;
        readonly LoggingOptions _loggingOptions;
        readonly RequestResponseContext _context;

        internal LoggingFactoryLogWriter(ILoggerFactory loggerFactory, LoggingOptions options)
        {
            _loggerFactory = loggerFactory;
            _loggingOptions = options;     
            MessageCreator = new LoggerFactoryMessageCreator(_loggingOptions, _context);
        }

        public ILogMessageCreator MessageCreator { get; }

        public async Task WriteAsync(RequestResponseContext context)
        {
            var message = MessageCreator.Create(context);
            _loggerFactory.CreateLogger(_loggingOptions.LoggerCategoryName).Log(_loggingOptions.LogLevel, message,(context));

            await Task.CompletedTask;

        }
    }
}
