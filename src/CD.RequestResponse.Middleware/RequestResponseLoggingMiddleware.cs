using System.Diagnostics;
using CD.RequestResponse.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CD.RequestResponse;

/// <summary>Captures a bounded, privacy-conscious snapshot without delaying or replacing the response body.</summary>
internal sealed class RequestResponseLoggingMiddleware
{
    private static readonly EventId ExchangeEvent = new(1000, "HttpExchange");
    private static readonly EventId SinkFailureEvent = new(1001, "RequestResponseSinkFailure");
    private static readonly Action<ILogger, string, Exception?> LogSinkFailure =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            SinkFailureEvent,
            "Request/response log sink {SinkName} failed. The HTTP response was not affected.");
    private static readonly Action<ILogger, Exception?> LogTransformFailure =
        LoggerMessage.Define(
            LogLevel.Warning,
            SinkFailureEvent,
            "A request/response body transform failed. The raw body was not logged.");
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly RequestResponseLoggingOptions _options;
    private readonly IReadOnlyList<IRequestResponseLogSink> _sinks;
    private readonly TimeProvider _timeProvider;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        IOptions<RequestResponseLoggingOptions> options,
        IEnumerable<IRequestResponseLogSink> sinks,
        ILogger<RequestResponseLoggingMiddleware> logger,
        TimeProvider timeProvider)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _sinks = sinks?.ToArray() ?? throw new ArgumentNullException(nameof(sinks));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!ShouldLog(context))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        Stream originalRequestBody = context.Request.Body;
        Stream originalResponseBody = context.Response.Body;
        long startedAt = Stopwatch.GetTimestamp();
        DateTimeOffset timestamp = _timeProvider.GetUtcNow();
        RequestCaptureStream? requestCapture = ShouldCapture(RequestResponseLogFields.RequestBody) &&
            BodyCapture.IsTextContentType(context.Request.ContentType)
                ? await RequestCaptureStream.CreateAsync(
                    originalRequestBody,
                    _options.RequestBodyLogLimit,
                    context.Request.ContentLength,
                    context.RequestAborted).ConfigureAwait(false)
                : null;
        ResponseCaptureStream? responseCapture = ShouldCapture(RequestResponseLogFields.ResponseBody)
            ? new ResponseCaptureStream(originalResponseBody, _options.ResponseBodyLogLimit)
            : null;
        Exception? pipelineException = null;
        if (requestCapture is not null)
        {
            context.Request.Body = requestCapture;
        }

        if (responseCapture is not null)
        {
            context.Response.Body = responseCapture;
        }

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            pipelineException = exception;
            throw;
        }
        finally
        {
            if (requestCapture is not null)
            {
                context.Request.Body = originalRequestBody;
            }

            if (responseCapture is not null)
            {
                context.Response.Body = originalResponseBody;
            }

            TimeSpan duration = Stopwatch.GetElapsedTime(startedAt);
            RequestResponseLogEntry entry = CreateEntry(
                context,
                timestamp,
                duration,
                requestCapture,
                responseCapture,
                pipelineException);

            await DispatchSafelyAsync(entry).ConfigureAwait(false);
            requestCapture?.Dispose();
            responseCapture?.Dispose();
        }
    }

    private bool ShouldLog(HttpContext context)
    {
        if (_options.ExcludedPathPrefixes.Any(prefix => context.Request.Path.StartsWithSegments(prefix)))
        {
            return false;
        }

        return _options.ShouldLog?.Invoke(context) ?? true;
    }

    private RequestResponseLogEntry CreateEntry(
        HttpContext context,
        DateTimeOffset timestamp,
        TimeSpan duration,
        RequestCaptureStream? requestCapture,
        ResponseCaptureStream? responseCapture,
        Exception? pipelineException)
    {
        string? responseBody = responseCapture is not null &&
            BodyCapture.IsTextContentType(context.Response.ContentType)
                ? TransformBody(context, responseCapture.GetCapturedText(), _options.ResponseBodyTransform)
                : null;
        string? requestBody = TransformBody(
            context,
            requestCapture?.GetCapturedText(),
            _options.RequestBodyTransform);

        return new RequestResponseLogEntry
        {
            Timestamp = Select(RequestResponseLogFields.Timestamp, timestamp),
            TraceIdentifier = Select(RequestResponseLogFields.TraceIdentifier, context.TraceIdentifier),
            Method = Select(RequestResponseLogFields.Method, context.Request.Method),
            Scheme = Select(RequestResponseLogFields.Scheme, context.Request.Scheme),
            Host = Select(RequestResponseLogFields.Host, context.Request.Host.Value),
            Path = Select(RequestResponseLogFields.Path, context.Request.Path.Value),
            QueryString = Select(RequestResponseLogFields.QueryString, context.Request.QueryString.Value),
            Protocol = Select(RequestResponseLogFields.Protocol, context.Request.Protocol),
            StatusCode = Select(RequestResponseLogFields.StatusCode, context.Response.StatusCode),
            Duration = Select(RequestResponseLogFields.Duration, duration),
            ClientIpAddress = Select(
                RequestResponseLogFields.ClientIpAddress,
                context.Connection.RemoteIpAddress?.ToString()),
            UserAgent = Select(RequestResponseLogFields.UserAgent, context.Request.Headers["User-Agent"].ToString()),
            RequestContentType = Select(
                RequestResponseLogFields.RequestContentType,
                context.Request.ContentType),
            ResponseContentType = Select(
                RequestResponseLogFields.ResponseContentType,
                context.Response.ContentType),
            RequestBody = requestBody,
            RequestBodyTruncated = requestCapture?.IsTruncated ?? false,
            ResponseBody = responseBody,
            ResponseBodyTruncated = responseCapture?.IsTruncated ?? false,
            ExceptionType = Select(RequestResponseLogFields.ExceptionType, pipelineException?.GetType().FullName),
            CompletedSuccessfully = pipelineException is null,
        };
    }

    private async Task DispatchSafelyAsync(RequestResponseLogEntry entry)
    {
        if (_options.EnableMicrosoftExtensionsLogging && _logger.IsEnabled(_options.LogLevel))
        {
            _logger.Log(
                _options.LogLevel,
                ExchangeEvent,
                new HttpExchangeLogState(entry),
                null,
                static (state, _) => state.ToString());
        }

        if (_options.Handler is not null)
        {
            await InvokeSafelyAsync(
                () => _options.Handler(entry, CancellationToken.None),
                "configured handler").ConfigureAwait(false);
        }

        foreach (IRequestResponseLogSink sink in _sinks)
        {
            await InvokeSafelyAsync(
                () => sink.WriteAsync(entry, CancellationToken.None),
                sink.GetType().FullName ?? sink.GetType().Name).ConfigureAwait(false);
        }
    }

    private async ValueTask InvokeSafelyAsync(Func<ValueTask> operation, string sinkName)
    {
        try
        {
            await operation().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogSinkFailure(_logger, sinkName, exception);
        }
    }

    private string? TransformBody(
        HttpContext context,
        string? body,
        Func<HttpContext, string, string>? transform)
    {
        if (body is null || transform is null)
        {
            return body;
        }

        try
        {
            return transform(context, body);
        }
        catch (Exception exception)
        {
            LogTransformFailure(_logger, exception);
            return "[Body transform failed]";
        }
    }

    private bool ShouldCapture(RequestResponseLogFields field)
        => (_options.Fields & field) == field;

    private string? Select(RequestResponseLogFields field, string? value)
        => ShouldCapture(field) ? value : null;

    private int? Select(RequestResponseLogFields field, int value)
        => ShouldCapture(field) ? value : null;

    private TimeSpan? Select(RequestResponseLogFields field, TimeSpan value)
        => ShouldCapture(field) ? value : null;

    private DateTimeOffset? Select(RequestResponseLogFields field, DateTimeOffset value)
        => ShouldCapture(field) ? value : null;
}
