namespace RRM_Library.Middlewares
{
    public abstract class BaseRequestResponseMiddleware
    {
        readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        readonly RequestResponseOptions _options;
        readonly ILogWriter _logWriter;

        protected async Task<RequestResponseContext> BaseMiddlewareInvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Request.EnableBuffering();
            var requestBody = await GetRequestBody(context.Request);

            var originalBodyStream = context.Response.Body;

            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            var sw = Stopwatch.StartNew();
            await next(context);
            sw.Stop();

            context.Request.Body.Seek(0, SeekOrigin.Begin);
            string responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Request.Body.Seek(0, SeekOrigin.Begin);

            var result = new RequestResponseContext(context)
            {
                Request = requestBody,
                RequestTime = TimeSpan.FromTicks(sw.ElapsedTicks),
                Response = responseBodyText
            };

            await _logWriter?.WriteAsync(result);
            return result;
        }

        // Reads the entire stream in chunks and returns it as a complete string.
        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;

            stream.Seek(0, SeekOrigin.Begin);

            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;

            do
            {
                readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }

        // Async reads the request body, buffering it for multiple reads, and returns the content as a string.
        private async Task<string> GetRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await request.Body.CopyToAsync(requestStream);
            string requestBody = ReadStreamInChunks(requestStream);
            request.Body.Seek(0, SeekOrigin.Begin);
            return requestBody;
        }
    }
}
