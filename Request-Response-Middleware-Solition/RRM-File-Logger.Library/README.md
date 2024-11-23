
# CD.File-Logger.Middleware

## Description

This project is an extension for logging HTTP request and response data to a file in ASP.NET Core applications. The middleware works by integrating with the [CD.RequestResponse.Middleware](https://www.nuget.org/packages/CD.RequestResponse.Middleware) package, which handles the basic request-response management, while this extension specifically handles logging to a file.

### Dependencies

- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [CD.RequestResponse.Middleware](https://www.nuget.org/packages/CD.RequestResponse.Middleware)
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection)
- [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions)
- [Microsoft.IO.RecyclableMemoryStream](https://www.nuget.org/packages/Microsoft.IO.RecyclableMemoryStream)

## Getting Started

This middleware adds the functionality to log request and response data directly to a file. It can be easily integrated with the `CD.RequestResponse.Middleware` package to provide comprehensive logging capabilities.

### Installation

To get started, add the `CD.File-Logger.Middleware` and `CD.RequestResponse.Middleware` NuGet packages to your project:

```bash
# Install CD.RequestResponse.Middleware
> dotnet add package CD.RequestResponse.Middleware
# Install CD.File-Logger.Middleware
> dotnet add package CD.File-Logger.Middleware
```

### Usage

After adding the necessary NuGet packages, you can configure the middleware in your `Program.cs` file. Here is an example demonstrating how to use the file logging middleware with `CD.RequestResponse.Middleware`.

```csharp
using Microsoft.Extensions.Logging;
using RRM_File_Logger.Library;
using RRM_Library;

var builder = WebApplication.CreateBuilder(args);

// Add Logging
builder.Services.AddLogging(opts =>
{
    opts.AddConsole();
});

// Add HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Adding RequestResponse Middleware with File Logger Middleware
app.AddRequestResponseMiddleware(opts =>
{
    opts.UseHandler(async context =>
    {
        Console.WriteLine("--Handler--
");
        Console.WriteLine($"Request: {context.Request}");
        Console.WriteLine($"Response: {context.Response}");
        await Task.CompletedTask;
    });
});

app.AddRequestResponseFileLoggerMiddleware(opts =>
{
    // You can specify the file path you want.
    opts.FileDirectory = AppDomain.CurrentDomain.BaseDirectory;
    opts.FileName = "alparslan_log";
    opts.Extension = ".txt";
    opts.UseJsonFormat = true;
    opts.ForceCreateDirectory = true;
});
```

### Parameters and Configuration

The `AddRequestResponseFileLoggerMiddleware` extension method allows you to configure the file logger options as follows:

- `FileDirectory`: Specify the directory where the log files will be saved.
- `FileName`: Set the base name of the log file.
- `Extension`: Set the extension of the log file (e.g., `.txt`, `.log`).
- `ForceCreateDirectory`: If set to `true`, the middleware will create the directory if it doesn't exist.

### Example Logging Output

Once the middleware is integrated, it will log each request and response to the specified file directory. Below is an example of how the log file might look:

```
datetime: 18.11.2024 13:25:30 - [GET /api/test] [200 OK] [Request Time: 00:00:01.025]
Request: ...
Response: ...
```

## Notes

- **Dependency**: The `CD.File-Logger.Middleware` package requires `CD.RequestResponse.Middleware` to function correctly. Be sure to install both.
- **Compatibility**: This package is compatible with .NET 8 and above.

## Contributing

Feel free to contribute by forking the repository and submitting pull requests. Any contributions are highly appreciated.

## License

This project is licensed under the MIT License. See the `LICENSE.txt` file for details.
