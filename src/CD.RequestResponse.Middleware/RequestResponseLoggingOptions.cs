using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CD.RequestResponse;

/// <summary>Configures privacy, filtering, body limits, and built-in logging.</summary>
public sealed class RequestResponseLoggingOptions
{
    public RequestResponseLogFields Fields { get; set; } = RequestResponseLogFields.Default;

    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    public int RequestBodyLogLimit { get; set; } = 4 * 1024;

    public int ResponseBodyLogLimit { get; set; } = 4 * 1024;

    public bool EnableMicrosoftExtensionsLogging { get; set; } = true;

    public ICollection<PathString> ExcludedPathPrefixes { get; } = new List<PathString>();

    public Func<HttpContext, bool>? ShouldLog { get; set; }

    public Func<HttpContext, string, string>? RequestBodyTransform { get; set; }

    public Func<HttpContext, string, string>? ResponseBodyTransform { get; set; }

    public Func<RequestResponseLogEntry, CancellationToken, ValueTask>? Handler { get; set; }
}
