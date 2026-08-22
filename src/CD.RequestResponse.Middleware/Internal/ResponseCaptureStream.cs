using System.Text;

namespace CD.RequestResponse.Internal;

internal sealed class ResponseCaptureStream : Stream
{
    private readonly MemoryStream _capture;
    private readonly Stream _inner;
    private readonly int _limit;
    private readonly object _sync = new();
    private long _totalWritten;

    public ResponseCaptureStream(Stream inner, int limit)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _limit = limit;
        _capture = new MemoryStream(Math.Min(limit, 16 * 1024));
    }

    public bool IsTruncated => _totalWritten > _limit;

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => _inner.CanSeek;

    public override bool CanWrite => _inner.CanWrite;

    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public string GetCapturedText()
    {
        lock (_sync)
        {
            return Encoding.UTF8.GetString(_capture.GetBuffer(), 0, checked((int)_capture.Length));
        }
    }

    public override void Flush() => _inner.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken)
        => _inner.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count)
        => _inner.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin)
        => _inner.Seek(offset, origin);

    public override void SetLength(long value) => _inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        _inner.Write(buffer, offset, count);
        Capture(buffer.AsSpan(offset, count));
    }

    public override async Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        await _inner.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        Capture(buffer.AsSpan(offset, count));
    }

    public override async ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        await _inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        Capture(buffer.Span);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _capture.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Capture(ReadOnlySpan<byte> buffer)
    {
        lock (_sync)
        {
            _totalWritten += buffer.Length;
            int remaining = _limit - checked((int)_capture.Length);

            if (remaining > 0)
            {
                _capture.Write(buffer[..Math.Min(buffer.Length, remaining)]);
            }
        }
    }
}
