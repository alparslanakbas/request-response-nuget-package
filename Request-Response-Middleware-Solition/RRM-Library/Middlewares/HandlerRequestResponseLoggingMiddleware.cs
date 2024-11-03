using Microsoft.AspNetCore.Http;
using RRM_Library.Models;

namespace RRM_Library.Middlewares
{
    public class HandlerRequestResponseLoggingMiddleware : BaseRequestResponseMiddleware
    {
        readonly Func<RequestResponseContext, Task> _requestResponseHandler;

        public HandlerRequestResponseLoggingMiddleware(Func<RequestResponseContext, Task> requestResponseHandler)
        {
            _requestResponseHandler = requestResponseHandler;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestResponseContext = await BaseMiddlewareInvokeAsync(context, next);
            
            await _requestResponseHandler.Invoke(requestResponseContext);
        }
    }
}
