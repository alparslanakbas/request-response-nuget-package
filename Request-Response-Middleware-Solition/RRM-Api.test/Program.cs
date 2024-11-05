using Microsoft.AspNetCore.Mvc;
using RRM_Library;
using RRM_Library.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
        Console.WriteLine($"Request: {context.Request}");
        Console.WriteLine($"Response: {context.Response}");
        Console.WriteLine($"Timer: {context.FormatedRequestTime}");
        Console.WriteLine($"Url: {context.Url}");
        await Task.CompletedTask;
    });
});

// GET
app.MapGet("/GetUserInfo/{id}", (int id) =>
{
    var response = new UserLoginResponseModel
    {
        Success = true,
        UserEmail = "alparslan@gmail.com"
    };
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
