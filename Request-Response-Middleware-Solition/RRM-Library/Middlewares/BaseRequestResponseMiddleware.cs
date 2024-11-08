using System.Net;
using System.Text.Json;

namespace RRM_Library.Middlewares
{
    public abstract class BaseRequestResponseMiddleware
    {
        readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        readonly ILogWriter _logWriter;
        readonly RequestDelegate next;
        readonly IHttpClientFactory _httpClientFactory;

        public BaseRequestResponseMiddleware(RequestDelegate next, ILogWriter logWriter, IHttpClientFactory httpClientFactory)
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _logWriter = logWriter;
            this.next = next;
            _httpClientFactory = httpClientFactory;
        }

        protected async Task<RequestResponseContext> BaseMiddlewareInvokeAsync(HttpContext context)
        {
            var requestBody = await ProcessRequestAsync(context);

            var responseContext = await ProcessResponseAsync(context, requestBody);

            await _logWriter?.WriteAsync(responseContext);
            return responseContext;
        }

        // Request processing
        private async Task<string> ProcessRequestAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            return await GetRequestBody(context);
        }

        // Response processing
        private async Task<RequestResponseContext> ProcessResponseAsync(HttpContext context, string requestBody)
        {
            var originalBodyStream = context.Response.Body;
            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            var sw = Stopwatch.StartNew();
            await next(context);
            sw.Stop();

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            return new RequestResponseContext(context)
            {
                Request = requestBody,
                RequestTime = TimeSpan.FromTicks(sw.ElapsedTicks),
                Response = responseBodyText,
                StatusCode = context.Response.StatusCode,
                Method = context.Request.Method,
                ClientIPAddress = GetClientIpAddress(context),
                ExternalIPAddress = await GetExternalIpAddressAsync(),
                HttpVersion = context.Request.Protocol,
                UserAgent = context.Request.Headers["User-Agent"],
                Cookies = FormatCookiesToJson(context.Request.Cookies)
            };
        }


        // Reads the entire stream in chunks and returns it as a complete string.
        private static async Task<string> ReadStreamAsync(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }


        // Async reads the request body, buffering it for multiple reads, and returns the content as a string.
        private async Task<string> GetRequestBody(HttpContext context)
        {
            context.Request.EnableBuffering();
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);

            string requestBody = await ReadStreamAsync(requestStream);
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            return requestBody;
        }

        // Helper for cookie formatting String
        private string FormatCookiesToString(IRequestCookieCollection cookies)
        {
            if (cookies is null || !cookies.Any())
            {
                return "No cookies";
            }

            var sb = new StringBuilder();
            foreach (var cookie in cookies)
            {
                sb.AppendFormat("{0}: {1}\n; ", cookie.Key, cookie.Value);
            }

            return sb.ToString();
        }

        // Helper for cookie formatting Json
        private string FormatCookiesToJson(IRequestCookieCollection cookies)
        {
            if (cookies is null || !cookies.Any())
            {
                return "{}";
            }

            var cookiesDictionary = new Dictionary<string, string>();
            return JsonSerializer.Serialize(cookiesDictionary);
        }

        // Client IP Address
        private string GetClientIpAddress(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        }

        // External IP Address
        private async Task<string> GetExternalIpAddressAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();
            return await httpClient.GetStringAsync("https://api.ipify.org");
        }
    }
}
