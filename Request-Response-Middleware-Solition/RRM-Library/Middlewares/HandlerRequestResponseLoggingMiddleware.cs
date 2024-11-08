namespace RRM_Library.Middlewares
{
    public class HandlerRequestResponseLoggingMiddleware : BaseRequestResponseMiddleware
    {
        readonly Func<RequestResponseContext, Task> _requestResponseHandler;
        
        public HandlerRequestResponseLoggingMiddleware(RequestDelegate next, Func<RequestResponseContext, Task> requestResponseHandler, ILogWriter logWriter, IHttpClientFactory httpClientFactory) : base(next, logWriter, httpClientFactory)
        {
            _requestResponseHandler = requestResponseHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestResponseContext = await BaseMiddlewareInvokeAsync(context);
            await _requestResponseHandler.Invoke(requestResponseContext);
        }
    }
}
