namespace RRM_Library.Models
{
    public class LoggingOptions
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public string LoggerCategoryName { get; set; } = "RequestResponseLoggerMiddleware";

        private List<LogFields> loggingFields;

        public List<LogFields> LoggingFields
        {
            get
            {
                return loggingFields ??= new List<LogFields>();
            }

            set => loggingFields = value;
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
