namespace RRM_Library.MessageCreators
{
    internal class LoggerFactoryMessageCreator : BaseLogMessageCreator, ILogMessageCreator
    {
        readonly LoggingOptions _loggingOptions;
        readonly RequestResponseContext context;

        public LoggerFactoryMessageCreator(LoggingOptions loggingOptions, RequestResponseContext context) : base(context)
        {
            _loggingOptions = loggingOptions;
            this.context = context;
        }

        public string Create(RequestResponseContext context)
        {
            var sb = new StringBuilder();

            foreach (var field in _loggingOptions.LoggingFields)
            {
                var value = GetValueByField(field);
                // output : Path: /api/values
                sb.AppendFormat("{0}: {1}\n", field, value);
            }

            return sb.ToString();
        }

        
    }
}