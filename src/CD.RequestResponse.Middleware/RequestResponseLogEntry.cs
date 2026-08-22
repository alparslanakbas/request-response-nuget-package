using System.Text.Json.Serialization;

namespace CD.RequestResponse;

/// <summary>Represents an immutable, bounded snapshot of one HTTP request and response.</summary>
public sealed class RequestResponseLogEntry
{
    public DateTimeOffset? Timestamp { get; init; }

    public string? TraceIdentifier { get; init; }

    public string? Method { get; init; }

    public string? Scheme { get; init; }

    public string? Host { get; init; }

    public string? Path { get; init; }

    public string? QueryString { get; init; }

    public string? Protocol { get; init; }

    public int? StatusCode { get; init; }

    [JsonIgnore]
    public TimeSpan? Duration { get; init; }

    public double? DurationMilliseconds => Duration?.TotalMilliseconds;

    public string? ClientIpAddress { get; init; }

    public string? UserAgent { get; init; }

    public string? RequestContentType { get; init; }

    public string? ResponseContentType { get; init; }

    public string? RequestBody { get; init; }

    public bool RequestBodyTruncated { get; init; }

    public string? ResponseBody { get; init; }

    public bool ResponseBodyTruncated { get; init; }

    public string? ExceptionType { get; init; }

    public bool CompletedSuccessfully { get; init; }
}
