

namespace RRM_Library
{
    public static class ApplicationBuiderExtensions
    {
        public static IApplicationBuilder AddRequestResponseMiddleware(this IApplicationBuilder appBuilder, Action<RequestResponseOptions> options)
        {
            var opt = new RequestResponseOptions();
            options(opt);

            ILogWriter logWriter = opt.LoggerFactory is null 
                ? new NullLogWriter()
                : new LoggingFactoryLogWriter(opt.LoggerFactory, opt.LoggingOptions);

            if (opt.RequestResponseHandler is not null)
                appBuilder.UseMiddleware<HandlerRequestResponseLoggingMiddleware>(opt.RequestResponseHandler);
            else
                appBuilder.UseMiddleware<RequestResponseLoggingMiddleware>(opt.RequestResponseHandler);

            return appBuilder;
        }
    }
}
