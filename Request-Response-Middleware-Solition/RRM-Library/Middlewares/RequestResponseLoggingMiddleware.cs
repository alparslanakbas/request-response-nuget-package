namespace RRM_Library.Middlewares
{
    public class RequestResponseLoggingMiddleware : BaseRequestResponseMiddleware
    {
        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogWriter logWriter, IHttpClientFactory httpClientFactory) : base(next, logWriter, httpClientFactory)
        {
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await BaseMiddlewareInvokeAsync(context);
        }
    }
}
