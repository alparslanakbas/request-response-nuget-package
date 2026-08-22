using CD.RequestResponse;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRequestResponseLogging(options =>
{
    options.Fields |= RequestResponseLogFields.RequestContentType |
        RequestResponseLogFields.ResponseContentType |
        RequestResponseLogFields.ExceptionType;
    options.ExcludedPathPrefixes.Add("/health");
});

builder.Services.AddRequestResponseFileLogging(options =>
{
    options.DirectoryPath = "logs";
    options.FileNamePrefix = "http";
});

WebApplication app = builder.Build();

app.UseRequestResponseLogging();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/orders/{id:int}", (int id) => Results.Ok(new { id, status = "ready" }));
app.MapPost("/orders", (CreateOrder request) => Results.Created("/orders/42", request));

app.Run();

internal sealed record CreateOrder(string Product, int Quantity);
