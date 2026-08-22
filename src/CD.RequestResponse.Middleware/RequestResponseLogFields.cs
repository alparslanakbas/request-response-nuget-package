namespace CD.RequestResponse;

/// <summary>Selects which HTTP exchange fields may be captured.</summary>
[Flags]
public enum RequestResponseLogFields
{
    None = 0,
    Timestamp = 1 << 0,
    TraceIdentifier = 1 << 1,
    Method = 1 << 2,
    Scheme = 1 << 3,
    Host = 1 << 4,
    Path = 1 << 5,
    QueryString = 1 << 6,
    Protocol = 1 << 7,
    StatusCode = 1 << 8,
    Duration = 1 << 9,
    ClientIpAddress = 1 << 10,
    UserAgent = 1 << 11,
    RequestContentType = 1 << 12,
    ResponseContentType = 1 << 13,
    RequestBody = 1 << 14,
    ResponseBody = 1 << 15,
    ExceptionType = 1 << 16,
    Default = Timestamp |
        TraceIdentifier |
        Method |
        Path |
        Protocol |
        StatusCode |
        Duration,
}
