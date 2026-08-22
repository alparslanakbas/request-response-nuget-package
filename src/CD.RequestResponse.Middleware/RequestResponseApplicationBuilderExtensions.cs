using CD.RequestResponse.Internal;
using Microsoft.AspNetCore.Builder;

namespace CD.RequestResponse;

/// <summary>Adds request and response observation to the ASP.NET Core pipeline.</summary>
public static class RequestResponseApplicationBuilderExtensions
{
    /// <summary>Adds the request and response logging middleware.</summary>
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.ApplicationServices.GetService(typeof(RequestResponseLoggingRegistrationMarker)) is null)
        {
            throw new InvalidOperationException(
                "Call services.AddRequestResponseLogging() before app.UseRequestResponseLogging().");
        }

        return app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
