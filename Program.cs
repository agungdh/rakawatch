using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rakawatch;
using Rakawatch.Models;
using Rakawatch.Services;
using System.Text.Json.Serialization;

AdminGuard.EnsureAdministrator();

var host = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://{host}:{port}");

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddProblemDetails();
builder.Services.AddHttpLogging();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<HardwareMonitorService>();
builder.Services.AddHostedService<HardwareSamplerService>();

var app = builder.Build();

app.UseHttpLogging();
app.UseCors();

var authToken = Environment.GetEnvironmentVariable("AUTH_TOKEN");
if (!string.IsNullOrEmpty(authToken))
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Headers.Authorization == $"Bearer {authToken}")
        {
            await next();
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new ErrorDto("Unauthorized."));
    });
}

app.MapOpenApi();
app.MapControllers();

Console.WriteLine($"Rakawatch running at http://{host}:{port}");
app.Run();