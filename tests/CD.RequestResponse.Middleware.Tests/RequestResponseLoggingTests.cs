using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CD.RequestResponse.Tests;

public sealed class RequestResponseLoggingTests
{
    [Fact]
    public async Task ResponseReachesClientUnchangedAndIsCaptured()
    {
        RecordingSink sink = new();
        await using TestApplication app = await CreateApplicationAsync(
            sink,
            options => options.Fields |= RequestResponseLogFields.ResponseBody,
            async context =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"ok\":true}");
            });

        string response = await app.Client.GetStringAsync("/orders");
        RequestResponseLogEntry entry = await sink.TakeAsync();

        Assert.Equal("{\"ok\":true}", response);
        Assert.Equal("{\"ok\":true}", entry.ResponseBody);
        Assert.False(entry.ResponseBodyTruncated);
    }

    [Fact]
    public async Task RequestBodyRemainsReadableByTheApplication()
    {
        RecordingSink sink = new();
        await using TestApplication app = await CreateApplicationAsync(
            sink,
            options => options.Fields |= RequestResponseLogFields.RequestBody,
            async context =>
            {
                using StreamReader reader = new(context.Request.Body, leaveOpen: true);
                string body = await reader.ReadToEndAsync(context.RequestAborted);
                await context.Response.WriteAsync(body);
            });

        using HttpResponseMessage response = await app.Client.PostAsJsonAsync(
            "/echo",
            new { message = "hello" });
        string responseBody = await response.Content.ReadAsStringAsync();
        RequestResponseLogEntry entry = await sink.TakeAsync();

        Assert.Contains("hello", responseBody, StringComparison.Ordinal);
        Assert.Contains("hello", entry.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SensitiveFieldsAreNotCapturedByDefault()
    {
        RecordingSink sink = new();
        await using TestApplication app = await CreateApplicationAsync(
            sink,
            configure: null,
            async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("secret response");
            });

        using HttpRequestMessage request = new(HttpMethod.Post, "/login?token=secret");
        request.Content = new StringContent("password=secret", Encoding.UTF8, "text/plain");
        using HttpResponseMessage _ = await app.Client.SendAsync(request);
        RequestResponseLogEntry entry = await sink.TakeAsync();

        Assert.Null(entry.QueryString);
        Assert.Null(entry.RequestBody);
        Assert.Null(entry.ResponseBody);
        Assert.Null(entry.ClientIpAddress);
        Assert.Null(entry.UserAgent);
    }

    [Fact]
    public async Task BodyCaptureIsBoundedAndReportsTruncation()
    {
        RecordingSink sink = new();
        await using TestApplication app = await CreateApplicationAsync(
            sink,
            options =>
            {
                options.Fields |= RequestResponseLogFields.RequestBody |
                    RequestResponseLogFields.ResponseBody;
                options.RequestBodyLogLimit = 4;
                options.ResponseBodyLogLimit = 5;
            },
            async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("123456789");
            });

        using HttpRequestMessage request = new(HttpMethod.Post, "/bounded");
        request.Content = new StringContent("abcdefgh", Encoding.UTF8, "text/plain");
        using HttpResponseMessage response = await app.Client.SendAsync(request);
        RequestResponseLogEntry entry = await sink.TakeAsync();

        Assert.Equal("123456789", await response.Content.ReadAsStringAsync());
        Assert.Equal("abcd", entry.RequestBody);
        Assert.True(entry.RequestBodyTruncated);
        Assert.Equal("12345", entry.ResponseBody);
        Assert.True(entry.ResponseBodyTruncated);
    }

    [Fact]
    public async Task BinaryResponseIsNotDecodedAsText()
    {
        RecordingSink sink = new();
        await using TestApplication app = await CreateApplicationAsync(
            sink,
            options => options.Fields |= RequestResponseLogFields.ResponseBody,
            async context =>
            {
                context.Response.ContentType = "application/octet-stream";
                await context.Response.Body.WriteAsync(new byte[] { 0, 1, 2, 3 });
            });

        byte[] response = await app.Client.GetByteArrayAsync("/download");
        RequestResponseLogEntry entry = await sink.TakeAsync();

        Assert.Equal(new byte[] { 0, 1, 2, 3 }, response);
        Assert.Null(entry.ResponseBody);
    }

    [Fact]
    public async Task ExcludedPathIsNotDispatched()
    {
        RecordingSink sink = new();
        await using TestApplication app = await CreateApplicationAsync(
            sink,
            options => options.ExcludedPathPrefixes.Add("/health"),
            context => context.Response.WriteAsync("healthy"));

        string response = await app.Client.GetStringAsync("/health/live");

        Assert.Equal("healthy", response);
        Assert.Empty(sink.Entries);
    }

    [Fact]
    public async Task SinkFailureNeverBreaksTheHttpResponse()
    {
        await using TestApplication app = await CreateApplicationAsync(
            new ThrowingSink(),
            configure: null,
            context => context.Response.WriteAsync("still works"));

        using HttpResponseMessage response = await app.Client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("still works", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task PipelineExceptionIsRecordedAndRethrown()
    {
        RecordingSink sink = new();
        await using TestApplication app = await CreateApplicationAsync(
            sink,
            options => options.Fields |= RequestResponseLogFields.ExceptionType,
            _ => throw new InvalidOperationException("Expected endpoint failure."));

        await Assert.ThrowsAnyAsync<Exception>(() => app.Client.GetAsync("/failure"));
        RequestResponseLogEntry entry = await sink.TakeAsync();

        Assert.False(entry.CompletedSuccessfully);
        Assert.Equal(typeof(InvalidOperationException).FullName, entry.ExceptionType);
    }

    [Fact]
    public async Task FailedBodyTransformNeverLeaksTheRawBody()
    {
        RecordingSink sink = new();
        await using TestApplication app = await CreateApplicationAsync(
            sink,
            options =>
            {
                options.Fields |= RequestResponseLogFields.RequestBody;
                options.RequestBodyTransform = (_, _) =>
                    throw new InvalidOperationException("Expected transform failure.");
            },
            context => context.Response.WriteAsync("ok"));

        using HttpRequestMessage request = new(HttpMethod.Post, "/login");
        request.Content = new StringContent("password=top-secret", Encoding.UTF8, "text/plain");
        using HttpResponseMessage _ = await app.Client.SendAsync(request);
        RequestResponseLogEntry entry = await sink.TakeAsync();

        Assert.Equal("[Body transform failed]", entry.RequestBody);
        Assert.DoesNotContain("top-secret", entry.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FileLoggingWritesValidJsonLinesOutsideTheRequestPath()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "cd-request-response-tests",
            Guid.NewGuid().ToString("N"));

        try
        {
            IHost host = await new HostBuilder()
                .ConfigureWebHost(builder => builder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddRequestResponseLogging(options =>
                            options.EnableMicrosoftExtensionsLogging = false);
                        services.AddRequestResponseFileLogging(options =>
                        {
                            options.DirectoryPath = directory;
                            options.FileNamePrefix = "integration";
                        });
                    })
                    .Configure(app =>
                    {
                        app.UseRequestResponseLogging();
                        app.Run(context => context.Response.WriteAsync("ok"));
                    }))
                .StartAsync();

            using HttpClient client = host.GetTestClient();
            await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => client.GetStringAsync("/orders")));
            await host.StopAsync();
            host.Dispose();

            string file = Assert.Single(Directory.GetFiles(directory, "integration-*.jsonl"));
            string[] lines = await File.ReadAllLinesAsync(file);

            Assert.Equal(20, lines.Length);

            foreach (string line in lines)
            {
                using JsonDocument json = JsonDocument.Parse(line);
                Assert.Equal("/orders", json.RootElement.GetProperty("path").GetString());
            }
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    private static async Task<TestApplication> CreateApplicationAsync(
        IRequestResponseLogSink sink,
        Action<RequestResponseLoggingOptions>? configure,
        RequestDelegate endpoint)
    {
        IHost host = await new HostBuilder()
            .ConfigureWebHost(builder => builder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddRequestResponseLogging(options =>
                    {
                        options.EnableMicrosoftExtensionsLogging = false;
                        configure?.Invoke(options);
                    });
                    services.AddSingleton(sink);
                })
                .Configure(app =>
                {
                    app.UseRequestResponseLogging();
                    app.Run(endpoint);
                }))
            .StartAsync();

        return new TestApplication(host, host.GetTestClient());
    }

    private sealed class TestApplication : IAsyncDisposable
    {
        private readonly IHost _host;

        public TestApplication(IHost host, HttpClient client)
        {
            _host = host;
            Client = client;
        }

        public HttpClient Client { get; }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    private sealed class RecordingSink : IRequestResponseLogSink
    {
        private readonly TaskCompletionSource<RequestResponseLogEntry> _next =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ConcurrentQueue<RequestResponseLogEntry> Entries { get; } = new();

        public ValueTask WriteAsync(
            RequestResponseLogEntry entry,
            CancellationToken cancellationToken = default)
        {
            Entries.Enqueue(entry);
            _next.TrySetResult(entry);
            return ValueTask.CompletedTask;
        }

        public Task<RequestResponseLogEntry> TakeAsync()
            => _next.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private sealed class ThrowingSink : IRequestResponseLogSink
    {
        public ValueTask WriteAsync(
            RequestResponseLogEntry entry,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Expected test failure.");
    }
}
