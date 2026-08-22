# Migrating from 1.x to 2.x

Version 2 is a deliberate redesign. It removes unsafe defaults and the old `RRM_Library` namespace, and includes file logging in `CD.RequestResponse.Middleware`.

## Package changes

| 1.x | 2.x |
| --- | --- |
| `CD.RequestResponse.Middleware` | Keep using this package. |
| `CD.File-Logger.Middleware` | Remove it after upgrading. It is now a compatibility package that depends on the main package. |

## Registration changes

Replace the old application-only configuration:

```csharp
app.AddRequestResponseMiddleware(options =>
{
    options.UseHandler(async context => { /* ... */ });
});
```

with service registration and middleware activation:

```csharp
builder.Services.AddRequestResponseLogging(options =>
{
    options.Handler = (entry, cancellationToken) =>
    {
        // entry contains only the selected fields.
        return ValueTask.CompletedTask;
    };
});

app.UseRequestResponseLogging();
```

`AddHttpClient()` is no longer required. Version 2 never calls an external service to discover the server's public IP address.

## File logging changes

Replace:

```csharp
app.AddRequestResponseFileLoggerMiddleware(options =>
{
    options.FileDirectory = "logs";
    options.FileName = "http";
});
```

with:

```csharp
builder.Services.AddRequestResponseFileLogging(options =>
{
    options.DirectoryPath = "logs";
    options.FileNamePrefix = "http";
});

app.UseRequestResponseLogging();
```

Version 2 emits one JSON object per line, uses daily rolling by default, and writes through a bounded background queue instead of performing file I/O during the request.

## Field mapping

| 1.x concept | 2.x equivalent |
| --- | --- |
| `RequestResponseContext.Request` | `RequestResponseLogEntry.RequestBody` (opt-in) |
| `RequestResponseContext.Response` | `RequestResponseLogEntry.ResponseBody` (opt-in) |
| `FormatedRequestTime` | `Duration` / `DurationMilliseconds` |
| `Url` | Select `Scheme`, `Host`, `Path`, and `QueryString` as needed. |
| `StatusCode` | `StatusCode` |
| `Method` | `Method` |
| `HttpVersion` | `Protocol` |
| `ClientIPAddress` | `ClientIpAddress` (opt-in) |
| `ExternalIPAddress` | Removed; an outbound lookup per request is not safe or reliable. |
| `UserAgent` | `UserAgent` (opt-in) |
| `Cookies` | Removed; cookies commonly contain authentication secrets. |
| `LoggingOptions.LogFields` | `RequestResponseLogFields` |

## Privacy changes

Bodies, query strings, IP addresses, user agents, and exception types are not selected by default. Body capture is supported only for textual content types, is bounded, and exposes truncation flags. Use body transforms to redact secrets before a handler or sink receives an entry.

Do not enable sensitive fields globally without reviewing consent, access control, retention, and deletion requirements for your application.
