using Microsoft.Extensions.Options;

namespace CD.RequestResponse.Internal;

internal sealed class RequestResponseLoggingOptionsValidator
    : IValidateOptions<RequestResponseLoggingOptions>
{
    private const int MaximumBodyLimit = 1024 * 1024;

    public ValidateOptionsResult Validate(string? name, RequestResponseLoggingOptions options)
    {
        if (options.RequestBodyLogLimit is < 1 or > MaximumBodyLimit)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options.RequestBodyLogLimit)} must be between 1 and {MaximumBodyLimit} bytes.");
        }

        if (options.ResponseBodyLogLimit is < 1 or > MaximumBodyLimit)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options.ResponseBodyLogLimit)} must be between 1 and {MaximumBodyLimit} bytes.");
        }

        return ValidateOptionsResult.Success;
    }
}

internal sealed class FileRequestResponseLoggingOptionsValidator
    : IValidateOptions<FileRequestResponseLoggingOptions>
{
    public ValidateOptionsResult Validate(string? name, FileRequestResponseLoggingOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DirectoryPath))
        {
            return ValidateOptionsResult.Fail("A file log directory is required.");
        }

        if (string.IsNullOrWhiteSpace(options.FileNamePrefix) ||
            options.FileNamePrefix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return ValidateOptionsResult.Fail("The file name prefix is invalid.");
        }

        if (options.QueueCapacity is < 1 or > 100_000)
        {
            return ValidateOptionsResult.Fail("Queue capacity must be between 1 and 100000.");
        }

        return ValidateOptionsResult.Success;
    }
}
