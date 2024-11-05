namespace RRM_Library.LogWriters
{
    internal class LoggingFactoryLogWriter : ILogWriter
    {
        readonly ILogger _logger;
        readonly LoggingOptions _loggingOptions;

        public ILogMessageCreator MessageCreator { get; }

        internal LoggingFactoryLogWriter(ILoggerFactory loggerFactory, LoggingOptions options)
        {
            _logger = loggerFactory.CreateLogger(options.LoggerCategoryName);
            _loggingOptions = options;     
            MessageCreator = new LoggerFactoryMessageCreator(options);
        }

        public async Task WriteAsync(RequestResponseContext context)
        {
            var message = MessageCreator.Create(context);
            _logger.Log(_loggingOptions.LogLevel, message);

            await Task.CompletedTask;

        }
    }
}
