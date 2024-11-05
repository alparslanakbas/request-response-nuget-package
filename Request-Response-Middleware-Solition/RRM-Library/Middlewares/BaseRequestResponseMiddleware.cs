namespace RRM_Library.Middlewares
{
    public abstract class BaseRequestResponseMiddleware
    {
        readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        readonly ILogWriter _logWriter;
        readonly RequestDelegate next;

        public BaseRequestResponseMiddleware(RequestDelegate next, ILogWriter logWriter)
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _logWriter = logWriter;
            this.next = next;
        }

        protected async Task<RequestResponseContext> BaseMiddlewareInvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            var requestBody = await GetRequestBody(context);

            var originalBodyStream = context.Response.Body;

            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            var sw = Stopwatch.StartNew();
            await next(context);
            sw.Stop();

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

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
        private async Task<string> GetRequestBody(HttpContext context)
        {
            context.Request.EnableBuffering();
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            string requestBody = ReadStreamInChunks(requestStream);
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            return requestBody;
        }
    }
}
