namespace CD.RequestResponse;

/// <summary>Configures asynchronous JSON Lines file output.</summary>
public sealed class FileRequestResponseLoggingOptions
{
    public string DirectoryPath { get; set; } = "logs";

    public string FileNamePrefix { get; set; } = "request-response";

    public bool RollDaily { get; set; } = true;

    public int QueueCapacity { get; set; } = 1_000;
}
