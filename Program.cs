using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rakawatch;
using Rakawatch.Services;
using System.Text.Json.Serialization;

if (!AdminGuard.IsAdministrator())
{
    AdminGuard.NotifyAndExit();
    return;
}

var host = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://{host}:{port}");

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddSingleton<HardwareMonitorService>();

var app = builder.Build();

app.UseCors();
app.MapControllers();

Console.WriteLine($"Rakawatch running at http://{host}:{port}");
app.Run();