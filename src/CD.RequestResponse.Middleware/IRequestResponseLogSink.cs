namespace CD.RequestResponse;

/// <summary>Receives completed HTTP exchange snapshots.</summary>
public interface IRequestResponseLogSink
{
    ValueTask WriteAsync(
        RequestResponseLogEntry entry,
        CancellationToken cancellationToken = default);
}
