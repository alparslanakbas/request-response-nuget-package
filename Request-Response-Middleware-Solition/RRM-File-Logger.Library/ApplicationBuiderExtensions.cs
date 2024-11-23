using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RRM_File_Logger.Library.LogWriters;
using RRM_File_Logger.Library.Models;
using RRM_Library.Interfaces;
using RRM_Library.Middlewares;

namespace RRM_File_Logger.Library
{
    public static class ApplicationBuiderExtensions
    {
        public static IApplicationBuilder AddRequestResponseFileLoggerMiddleware(this IApplicationBuilder appBuilder, Action<FileLoggingOptions> options)
        {
            var opt = new FileLoggingOptions();
            options(opt);

            ILogWriter logWriter = new FileLogWriter(opt);

            IHttpClientFactory httpClientFactory = appBuilder.ApplicationServices.GetRequiredService<IHttpClientFactory>();

            appBuilder.UseMiddleware<RequestResponseLoggingMiddleware>(logWriter,httpClientFactory);

            return appBuilder;
        }
    }
}
