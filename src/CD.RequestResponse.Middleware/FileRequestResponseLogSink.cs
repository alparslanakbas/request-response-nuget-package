using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CD.RequestResponse;

/// <summary>Queues entries and writes them as JSON Lines outside the request path.</summary>
internal sealed class FileRequestResponseLogSink : BackgroundService, IRequestResponseLogSink
{
    private static readonly Action<ILogger, Exception?> LogQueueFull =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(2000, "FileLogQueueFull"),
            "The request/response file log queue is full; one entry was dropped.");
    private static readonly Action<ILogger, Exception?> LogWriteFailure =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2001, "FileLogWriteFailure"),
            "Unable to write a request/response file log entry.");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly Channel<RequestResponseLogEntry> _channel;
    private readonly ILogger<FileRequestResponseLogSink> _logger;
    private readonly FileRequestResponseLoggingOptions _options;

    public FileRequestResponseLogSink(
        IOptions<FileRequestResponseLoggingOptions> options,
        ILogger<FileRequestResponseLogSink> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _channel = Channel.CreateBounded<RequestResponseLogEntry>(new BoundedChannelOptions(_options.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public ValueTask WriteAsync(
        RequestResponseLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!_channel.Writer.TryWrite(entry))
        {
            LogQueueFull(_logger, null);
        }

        return ValueTask.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string directory = Path.GetFullPath(_options.DirectoryPath);
        Directory.CreateDirectory(directory);

        await foreach (RequestResponseLogEntry entry in
            _channel.Reader.ReadAllAsync(CancellationToken.None))
        {
            try
            {
                string path = GetFilePath(directory, entry.Timestamp ?? DateTimeOffset.UtcNow);
                string line = JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine;
                await File.AppendAllTextAsync(path, line, Encoding.UTF8, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                LogWriteFailure(_logger, exception);
            }
        }
    }

    private string GetFilePath(string directory, DateTimeOffset timestamp)
    {
        string suffix = _options.RollDaily ? $"-{timestamp.UtcDateTime:yyyyMMdd}" : string.Empty;
        return Path.Combine(directory, $"{_options.FileNamePrefix}{suffix}.jsonl");
    }
}
