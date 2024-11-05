using static RRM_Library.Models.LoggingOptions;

namespace RRM_Library.MessageCreators
{
    public abstract class BaseLogMessageCreator
    {
        private readonly RequestResponseContext _context;

        protected BaseLogMessageCreator(RequestResponseContext context)
        {
            _context = context;
        }

        protected string GetValueByField(LogFields fields)
        {
            return fields switch
            {
                LogFields.Request => _context.Request ?? string.Empty,
                LogFields.Response => _context.Response ?? string.Empty,
                LogFields.RequestLength => _context.RequestLength?.ToString() ?? string.Empty,
                LogFields.ResponseLength => _context.ResponseLength?.ToString() ?? string.Empty,
                LogFields.QueryString => _context._context.Request.QueryString.Value ?? string.Empty,
                LogFields.Path => _context._context.Request.Path.Value ?? string.Empty,
                LogFields.HostName => _context._context.Request.Host.Value ?? string.Empty,
                LogFields.ResponseTime => _context.FormatedRequestTime ?? string.Empty,
                _ => string.Empty
            };
        }
    }
}
