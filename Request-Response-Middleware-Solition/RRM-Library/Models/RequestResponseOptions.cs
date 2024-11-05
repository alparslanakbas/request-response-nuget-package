namespace RRM_Library.Models
{
    public class RequestResponseOptions
    {
        private LoggingOptions _loggingOptions = new();
        private ILoggerFactory? _loggerFactory;
        private Func<RequestResponseContext, Task>? _requestResponseHandler;

        
        internal Func<RequestResponseContext, Task>? RequestResponseHandler
        {
            get => _requestResponseHandler;
            set => _requestResponseHandler = value;
        }

        
        internal ILoggerFactory LoggerFactory
        {
            get => _loggerFactory ?? throw new InvalidOperationException("LoggerFactory is not initialized");
            set => _loggerFactory = value;
        }

        
        internal LoggingOptions LoggingOptions
        {
            get => _loggingOptions;
            set => _loggingOptions = value ?? throw new ArgumentNullException(nameof(value));
        }


        public void UseHandler(Func<RequestResponseContext, Task> handler)
        {
            RequestResponseHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        }


        public void UseLogger(ILoggerFactory loggerFactory, Action<LoggingOptions>? loggingAction = null)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory);

            LoggerFactory = loggerFactory;
            loggingAction?.Invoke(_loggingOptions);
        }
    }
}
