# CD.RequestResponse.Middleware

[![NuGet](https://img.shields.io/nuget/v/CD.RequestResponse.Middleware.svg)](https://www.nuget.org/packages/CD.RequestResponse.Middleware)
[![CI](https://github.com/alparslanakbas/request-response-nuget-package/actions/workflows/ci.yml/badge.svg)](https://github.com/alparslanakbas/request-response-nuget-package/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Privacy-conscious ASP.NET Core request/response logging for .NET 8, 9, and 10. It provides structured `ILogger` output, custom sinks and handlers, bounded opt-in body capture, and asynchronous JSON Lines file logging.

## Why this package?

- Safe defaults: bodies, query strings, client IP addresses, and user agents are not captured unless explicitly enabled.
- Bounded text-body capture avoids unbounded memory usage; binary bodies are never decoded as text.
- The response is tee'd to the client while it is observed, so logging does not replace or delay the response stream.
- Sink and handler failures never break a successful HTTP response.
- File logging uses a bounded background queue and daily `.jsonl` files.
- No external IP lookup and no extra runtime NuGet dependencies.

## Install

```bash
dotnet add package CD.RequestResponse.Middleware --prerelease
```

## Quick start

```csharp
using CD.RequestResponse;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRequestResponseLogging(options =>
{
    options.ExcludedPathPrefixes.Add("/health");
});

var app = builder.Build();

app.UseRequestResponseLogging();

app.MapGet("/orders/{id:int}", (int id) => Results.Ok(new { id }));
app.Run();
```

The default fields are timestamp, trace identifier, method, path, protocol, status code, and duration. Output is sent through `Microsoft.Extensions.Logging`, so it works with console logging, OpenTelemetry, Serilog, Application Insights, and other configured providers.

## Asynchronous file logging

File logging is included in the main package:

```csharp
builder.Services.AddRequestResponseFileLogging(options =>
{
    options.DirectoryPath = "logs";
    options.FileNamePrefix = "http";
    options.RollDaily = true;
    options.QueueCapacity = 1_024;
});
```

Each line is an independent JSON document. The legacy `CD.File-Logger.Middleware` package remains only as a compatibility package that installs this main package.

## Opting into bodies

Request and response bodies can contain passwords, tokens, personal data, or payment information. Enable them only when you have a clear data-retention policy and redact secrets before dispatch:

```csharp
builder.Services.AddRequestResponseLogging(options =>
{
    options.Fields |= RequestResponseLogFields.RequestBody |
        RequestResponseLogFields.ResponseBody;
    options.RequestBodyLogLimit = 8 * 1024;
    options.ResponseBodyLogLimit = 8 * 1024;

    options.RequestBodyTransform = (context, body) => RedactSecrets(body);
    options.ResponseBodyTransform = (context, body) => RedactSecrets(body);
});
```

Only known textual media types such as JSON, XML, form URL encoded data, GraphQL, and `text/*` are eligible. Limits must be between 1 byte and 1 MiB.

## Choose fields and filter requests

```csharp
builder.Services.AddRequestResponseLogging(options =>
{
    options.Fields = RequestResponseLogFields.Default |
        RequestResponseLogFields.Host |
        RequestResponseLogFields.ResponseContentType |
        RequestResponseLogFields.ExceptionType;

    options.ShouldLog = context => !context.Request.Path.StartsWithSegments("/metrics");
});
```

`ClientIpAddress` uses only ASP.NET Core's `RemoteIpAddress`; it does not trust forwarded headers directly. If the app is behind a trusted proxy, configure ASP.NET Core Forwarded Headers middleware before `UseRequestResponseLogging`.

## Custom handler or sink

For a small integration, configure a handler:

```csharp
builder.Services.AddRequestResponseLogging(options =>
{
    options.Handler = (entry, cancellationToken) =>
    {
        // Send the immutable snapshot to your own destination.
        return ValueTask.CompletedTask;
    };
});
```

For a reusable destination, implement `IRequestResponseLogSink` and register it once:

```csharp
builder.Services.AddRequestResponseLogSink<MyAuditSink>();
```

Sink failures are logged as warnings and isolated from the HTTP pipeline.

## Middleware order

Place forwarded-header handling first, then this middleware before endpoints. Place exception handling before or after it depending on whether you want the transformed error response or the original exception outcome to be observed.

```csharp
app.UseForwardedHeaders();
app.UseRequestResponseLogging();
app.UseExceptionHandler();
```

## Version 2 migration

Version 2 replaces the original `RRM_Library` API and merges file logging into the main package. See [the migration guide](docs/MIGRATION_V2.md) for exact mappings and behavior changes.

## Development

```bash
dotnet restore CD.RequestResponse.sln
dotnet build CD.RequestResponse.sln -c Release --no-restore
dotnet test CD.RequestResponse.sln -c Release --no-build
```

The repository includes an executable [sample API](samples/CD.RequestResponse.SampleApi) and integration tests based on ASP.NET Core TestServer.
Maintainers can follow the [trusted publishing release guide](docs/RELEASING.md).

## License

[MIT](LICENSE)
