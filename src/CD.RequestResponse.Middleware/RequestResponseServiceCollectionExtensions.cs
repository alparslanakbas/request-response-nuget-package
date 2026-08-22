using CD.RequestResponse.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CD.RequestResponse;

/// <summary>Registers request and response observation services.</summary>
public static class RequestResponseServiceCollectionExtensions
{
    /// <summary>Registers request and response logging with privacy-conscious defaults.</summary>
    public static IServiceCollection AddRequestResponseLogging(
        this IServiceCollection services,
        Action<RequestResponseLoggingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        OptionsBuilder<RequestResponseLoggingOptions> options =
            services.AddOptions<RequestResponseLoggingOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        options.ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<RequestResponseLoggingOptions>,
                RequestResponseLoggingOptionsValidator>());
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<RequestResponseLoggingRegistrationMarker>();

        return services;
    }

    /// <summary>Registers a custom log sink.</summary>
    public static IServiceCollection AddRequestResponseLogSink<TSink>(this IServiceCollection services)
        where TSink : class, IRequestResponseLogSink
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IRequestResponseLogSink, TSink>());
        return services;
    }

    /// <summary>Writes entries to bounded, asynchronous JSON Lines files.</summary>
    public static IServiceCollection AddRequestResponseFileLogging(
        this IServiceCollection services,
        Action<FileRequestResponseLoggingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddRequestResponseLogging();

        OptionsBuilder<FileRequestResponseLoggingOptions> options =
            services.AddOptions<FileRequestResponseLoggingOptions>();

        if (configure is not null)
        {
            options.Configure(configure);
        }

        options.ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<FileRequestResponseLoggingOptions>,
                FileRequestResponseLoggingOptionsValidator>());
        if (!services.Any(descriptor =>
            descriptor.ServiceType == typeof(FileRequestResponseLoggingRegistrationMarker)))
        {
            services.AddSingleton<FileRequestResponseLoggingRegistrationMarker>();
            services.AddSingleton<FileRequestResponseLogSink>();
            services.AddSingleton<IRequestResponseLogSink>(provider =>
                provider.GetRequiredService<FileRequestResponseLogSink>());
            services.AddSingleton<IHostedService>(provider =>
                provider.GetRequiredService<FileRequestResponseLogSink>());
        }

        return services;
    }
}
