namespace RRM_Library.Models
{
    public class LoggingOptions
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public string LoggerCategoryName { get; set; } = "RequestResponseLoggerMiddleware";
        public LogFields LoggingFields { get; set; } = LogFields.None;

        [Flags]
        public enum LogFields
        {
            None = 0,
            Request = 1 << 0,
            Response = 1 << 1,
            HostName = 1 << 2,
            Path = 1 << 3,
            QueryString = 1 << 4,
            ResponseTime = 1 << 5,
            RequestLength = 1 << 6,
            ResponseLength = 1 << 7,
            StatusCode = 1 << 8
        }
    }

}
