using System.Collections;

namespace CD.RequestResponse.Internal;

internal sealed class HttpExchangeLogState : IReadOnlyList<KeyValuePair<string, object?>>
{
    private const string MessageTemplate =
        "{Method} {Path} responded {StatusCode} in {DurationMilliseconds} ms";
    private readonly RequestResponseLogEntry _entry;

    public HttpExchangeLogState(RequestResponseLogEntry entry)
    {
        _entry = entry;
    }

    public int Count => 6;

    public KeyValuePair<string, object?> this[int index] => index switch
    {
        0 => new("Method", _entry.Method),
        1 => new("Path", _entry.Path),
        2 => new("StatusCode", _entry.StatusCode),
        3 => new("DurationMilliseconds", _entry.DurationMilliseconds),
        4 => new("HttpExchange", _entry),
        5 => new("{OriginalFormat}", MessageTemplate),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (int index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
        => $"{_entry.Method} {_entry.Path} responded {_entry.StatusCode} " +
            $"in {_entry.DurationMilliseconds} ms";
}
