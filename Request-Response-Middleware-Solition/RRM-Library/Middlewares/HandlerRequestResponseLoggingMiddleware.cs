namespace RRM_Library.Middlewares
{
    public class HandlerRequestResponseLoggingMiddleware : BaseRequestResponseMiddleware
    {
        readonly Func<RequestResponseContext, Task> _requestResponseHandler;
        readonly ILogWriter _logWriter;

        public HandlerRequestResponseLoggingMiddleware(Func<RequestResponseContext, Task> requestResponseHandler, ILogWriter logWriter)
        {
            _requestResponseHandler = requestResponseHandler;
            _logWriter = logWriter;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await BaseMiddlewareInvokeAsync(context, next);
        }
    }
}
