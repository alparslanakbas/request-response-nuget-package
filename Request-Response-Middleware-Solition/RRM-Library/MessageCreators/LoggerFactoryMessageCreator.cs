namespace RRM_Library.MessageCreators
{
    internal class LoggerFactoryMessageCreator : BaseLogMessageCreator, ILogMessageCreator
    {
        readonly LoggingOptions _loggingOptions;

        public LoggerFactoryMessageCreator(LoggingOptions loggingOptions)
        {
            _loggingOptions = loggingOptions;
        }

        public string Create(RequestResponseContext context)
        {
            var sb = new StringBuilder();

            foreach (var field in _loggingOptions.LoggingFields)
            {
                var value = GetValueByField(context, field);
                // output : Path: /api/values
                sb.AppendFormat("{0}: {1}\n", field, value);
            }

            return sb.ToString();
        }

        
    }
}