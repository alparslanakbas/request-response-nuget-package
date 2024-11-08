using static RRM_Library.Models.LoggingOptions;

namespace RRM_Library.MessageCreators
{
    public abstract class BaseLogMessageCreator
    {
        protected string GetValueByField(RequestResponseContext context, LogFields fields)
        {
            return fields switch
            {
                LogFields.Request => context.Request ?? string.Empty,
                LogFields.Response => context.Response ?? string.Empty,
                LogFields.RequestLength => context.RequestLength?.ToString() ?? string.Empty,
                LogFields.ResponseLength => context.ResponseLength?.ToString() ?? string.Empty,
                LogFields.QueryString => context._context.Request.QueryString.Value ?? string.Empty,
                LogFields.Path => context._context.Request.Path.Value ?? string.Empty,
                LogFields.HostName => context._context.Request.Host.Value ?? string.Empty,
                LogFields.ResponseTime => context.FormatedRequestTime ?? string.Empty,
                LogFields.StatusCode => context.StatusCode.ToString(),
                _ => string.Empty
            };
        }
    }
}
