namespace RRM_Library
{
    public class RequestResponseOptions
    {
        internal Func<RequestResponseContext, Task> RequestResponseHandler { get; set; }
        internal ILoggerFactory LoggerFactory;
        internal LoggingOptions LoggingOption;

        public void UseHandler(Func<RequestResponseContext, Task> handler)
        {
            RequestResponseHandler = handler;
        }

        public void UseLogger(ILoggerFactory loggerFactory, Action<LoggingOptions> loggingAction)
        {
            LoggingOption = new LoggingOptions();
            loggingAction(LoggingOption);
            LoggerFactory = loggerFactory;
        }
    }
}
