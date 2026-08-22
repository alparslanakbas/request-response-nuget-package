using System.Buffers;
using System.Text;

namespace CD.RequestResponse.Internal;

internal sealed class RequestCaptureStream : Stream
{
    private readonly Stream _inner;
    private readonly int _limit;
    private readonly byte[] _prefix;
    private int _prefixPosition;

    private RequestCaptureStream(Stream inner, int limit, byte[] prefix, bool isTruncated)
    {
        _inner = inner;
        _limit = limit;
        _prefix = prefix;
        IsTruncated = isTruncated;
    }

    public bool IsTruncated { get; }

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => _inner.CanWrite;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public static async Task<RequestCaptureStream> CreateAsync(
        Stream inner,
        int limit,
        long? contentLength,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inner);

        int targetLength = contentLength.HasValue
            ? checked((int)Math.Min(contentLength.Value, limit + 1L))
            : limit + 1;
        byte[] rented = ArrayPool<byte>.Shared.Rent(Math.Max(targetLength, 1));

        try
        {
            int totalRead = 0;

            while (totalRead < targetLength)
            {
                int read = await inner.ReadAsync(
                    rented.AsMemory(totalRead, targetLength - totalRead),
                    cancellationToken).ConfigureAwait(false);

                if (read == 0)
                {
                    break;
                }

                totalRead += read;
            }

            byte[] prefix = rented.AsSpan(0, totalRead).ToArray();
            bool isTruncated = totalRead > limit || contentLength > limit;
            return new RequestCaptureStream(inner, limit, prefix, isTruncated);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    public string? GetCapturedText()
    {
        int capturedLength = Math.Min(_prefix.Length, _limit);
        return capturedLength == 0
            ? null
            : Encoding.UTF8.GetString(_prefix, 0, capturedLength);
    }

    public override void Flush() => _inner.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken)
        => _inner.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count)
    {
        int replayed = Replay(buffer.AsSpan(offset, count));
        return replayed > 0 ? replayed : _inner.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        int replayed = Replay(buffer);
        return replayed > 0 ? replayed : _inner.Read(buffer);
    }

    public override Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        int replayed = Replay(buffer.AsSpan(offset, count));
        return replayed > 0
            ? Task.FromResult(replayed)
            : _inner.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        int replayed = Replay(buffer.Span);
        return replayed > 0
            ? ValueTask.FromResult(replayed)
            : _inner.ReadAsync(buffer, cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    public override void SetLength(long value) => _inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
        => _inner.Write(buffer, offset, count);

    public override Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
        => _inner.WriteAsync(buffer, offset, count, cancellationToken);

    public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
        => _inner.WriteAsync(buffer, cancellationToken);

    private int Replay(Span<byte> destination)
    {
        int remaining = _prefix.Length - _prefixPosition;

        if (remaining == 0 || destination.Length == 0)
        {
            return 0;
        }

        int count = Math.Min(remaining, destination.Length);
        _prefix.AsSpan(_prefixPosition, count).CopyTo(destination);
        _prefixPosition += count;
        return count;
    }
}
