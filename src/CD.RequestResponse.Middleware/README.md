# CD.RequestResponse.Middleware

Privacy-conscious ASP.NET Core request/response logging with structured logs, bounded opt-in body capture, custom sinks, and asynchronous JSON Lines file output. Supports .NET 8, 9, and 10.

```csharp
using CD.RequestResponse;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRequestResponseLogging(options =>
{
    options.ExcludedPathPrefixes.Add("/health");
});

builder.Services.AddRequestResponseFileLogging(options =>
{
    options.DirectoryPath = "logs";
});

var app = builder.Build();
app.UseRequestResponseLogging();
```

Bodies, query strings, client IP addresses, and user agents are disabled by default. Body capture must be explicitly enabled and is limited to known textual content types:

```csharp
options.Fields |= RequestResponseLogFields.RequestBody |
    RequestResponseLogFields.ResponseBody;
options.RequestBodyLogLimit = 8 * 1024;
options.ResponseBodyLogLimit = 8 * 1024;
options.RequestBodyTransform = (context, body) => RedactSecrets(body);
```

Use `options.Handler` for a small custom integration or implement `IRequestResponseLogSink` and register it with `AddRequestResponseLogSink<TSink>()`.

See the [complete documentation and migration guide](https://github.com/alparslanakbas/request-response-nuget-package) on GitHub.
