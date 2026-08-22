namespace CD.RequestResponse.Internal;

internal static class BodyCapture
{
    public static bool IsTextContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        string mediaType = contentType.Split(';', 2)[0].Trim();
        return mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("application/xml", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("application/graphql", StringComparison.OrdinalIgnoreCase) ||
            mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase) ||
            mediaType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);
    }
}
