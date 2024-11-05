namespace RRM_Library
{
    public static class ApplicationBuiderExtensions
    {
        public static IApplicationBuilder AddRequestResponseMiddleware(this IApplicationBuilder appBuilder, Action<RequestResponseOptions> options)
        {
            var opt = new RequestResponseOptions();
            options(opt);

            if (opt.RequestResponseHandler is null && opt.LoggerFactory is null)
                throw new ArgumentNullException($"{nameof(opt.RequestResponseHandler)} and {nameof(opt.LoggerFactory)}");

            ILogWriter logWriter = opt.LoggerFactory is null 
                ? new NullLogWriter()
                : new LoggingFactoryLogWriter(opt.LoggerFactory, opt.LoggingOption);

            if (opt.RequestResponseHandler is not null)
                appBuilder.UseMiddleware<HandlerRequestResponseLoggingMiddleware>(opt.RequestResponseHandler, logWriter);
            else
                appBuilder.UseMiddleware<RequestResponseLoggingMiddleware>(logWriter);

            return appBuilder;
        }
    }
}
