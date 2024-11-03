using Microsoft.AspNetCore.Http;

namespace RRM_Library.Middlewares
{
    public class RequestResponseLoggingMiddleware : BaseRequestResponseMiddleware
    {

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
           var requestResponseContext = BaseMiddlewareInvokeAsync(context, next);


        }
    }
}
