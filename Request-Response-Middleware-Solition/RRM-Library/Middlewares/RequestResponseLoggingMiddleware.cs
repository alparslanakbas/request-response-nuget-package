namespace RRM_Library.Middlewares
{
    public class RequestResponseLoggingMiddleware : BaseRequestResponseMiddleware
    {
        readonly ILogWriter _logWriter;

        public RequestResponseLoggingMiddleware(ILogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await BaseMiddlewareInvokeAsync(context, next);
        }
    }
}
