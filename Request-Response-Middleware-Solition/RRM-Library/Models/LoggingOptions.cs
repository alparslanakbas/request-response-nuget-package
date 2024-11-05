namespace RRM_Library.Models
{
    public class LoggingOptions
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public string LoggerCategoryName { get; set; } = "RequestResponseLoggerMiddleware";

        private List<LogFields> _loggingFields = new();

        public IReadOnlyList<LogFields> LoggingFields
        {
            get
            {
                return _loggingFields ??= new List<LogFields>();
            }
            init => _loggingFields = value?.ToList() ?? new List<LogFields>();
        }

        public enum LogFields
        {
            Request,
            Response,
            HostName,
            Path,
            QueryString,
            ResponseTime,
            RequestLength,
            ResponseLength,
            StatusCode
        }
    }
}
