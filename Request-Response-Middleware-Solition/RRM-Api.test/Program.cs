using Microsoft.AspNetCore.Mvc;
using RRM_Library;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddLogging(opts =>
{
    opts.AddConsole();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.AddRequestResponseMiddleware(opts =>
{ 
    opts.UseHandler(async context =>
    {
        Console.WriteLine("--Handler--\n");
        Console.WriteLine($"Request: {context.Request}");
        Console.WriteLine($"Response: {context.Response}");
        Console.WriteLine($"Timer: {context.FormatedRequestTime}");
        Console.WriteLine($"Url: {context.Url}");
        Console.WriteLine($"Status Code: {context.StatusCode}");
        Console.WriteLine($"Method: {context.Method}");
        Console.WriteLine($"HTTP Version: {context.HttpVersion}");
        Console.WriteLine($"Client IP Address: {context.ClientIPAddress}");
        Console.WriteLine($"External IP Address: {context.ExternalIPAddress}");
        Console.WriteLine($"User Agent: {context.UserAgent}");
        Console.WriteLine($"Cookies: {context.Cookies}");

        await Task.CompletedTask;
    });

    
    opts.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), opts =>
    {
        opts.LogLevel = LogLevel.Error;
        opts.LoggerCategoryName = "RRM-Api-Test";

        opts.LoggingFields = 
                         RRM_Library.Models.LoggingOptions.LogFields.Request |
                         RRM_Library.Models.LoggingOptions.LogFields.Response |
                         RRM_Library.Models.LoggingOptions.LogFields.ResponseTime |
                         RRM_Library.Models.LoggingOptions.LogFields.StatusCode |
                         RRM_Library.Models.LoggingOptions.LogFields.HostName |
                         RRM_Library.Models.LoggingOptions.LogFields.Path |
                         RRM_Library.Models.LoggingOptions.LogFields.QueryString 
                         ;
    });
});

// GET
app.MapGet("/GetUserInfo/{id}", (int id, ILogger<Program> logger) =>
{
        var response = new UserLoginResponseModel
        {
            Success = true,
            UserEmail = "alparslan@gmail.com"
        };
        logger.LogInformation("User info is requested");
        return Results.Ok(response);
})
.WithName("GetUserInfo")
.WithOpenApi();

// POST
app.MapPost("/Login", ([FromBody]UserLoginRequestModel request) =>
{
var response = new UserLoginResponseModel
{
    Success = true,
    UserEmail = "alparslan@gmail.com"
};
    return Results.Ok(response);
})
.WithName("Login")
.WithOpenApi();

// login only
app.MapPost("/LoginOnly", ([FromBody] UserLoginRequestModel request) =>
{
    return Results.Ok();
})
.WithName("LoginOnly")
.WithOpenApi();

app.Run();

internal class UserLoginRequestModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

internal class UserLoginResponseModel
{
    public bool Success { get; set; }
    public string UserEmail { get; set; }
}
