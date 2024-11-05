namespace RRM_Library.Middlewares
{
    public class RequestResponseLoggingMiddleware : BaseRequestResponseMiddleware
    {
        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogWriter logWriter): base(next, logWriter)
        {
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await BaseMiddlewareInvokeAsync(context);
        }
    }
}
