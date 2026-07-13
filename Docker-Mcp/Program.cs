using System.Runtime.InteropServices;
using Docker.DotNet;
using Docker_Mcp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleOptions => { consoleOptions.LogToStandardErrorThreshold = LogLevel.Trace; });
builder.Logging.AddSimpleConsole(formatterOptions =>
{
    formatterOptions.IncludeScopes = false;
    formatterOptions.TimestampFormat = "[HH:mm:ss] ";
});

builder.Logging.AddFilter(_ => true);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.Warning);
builder.Logging.AddFilter("ModelContextProtocol", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Services.AddSingleton<DockerService>();

builder.Services.AddSingleton(_ =>
{
    Uri dockerUri;

    if (OperatingSystem.IsWindows())
        dockerUri = new Uri("npipe://./pipe/docker_engine");
    else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        dockerUri = new Uri("unix:///var/run/docker.sock");
    else
        throw new PlatformNotSupportedException(
            $"Docker is not supported on this operating system " +
            $"({RuntimeInformation.OSDescription}). " +
            "Supported systems: Windows, Linux, and macOS.");

    return new Lazy<DockerClient?>(() =>
    {
        try
        {
            return new DockerClientConfiguration(dockerUri).CreateClient();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Docker] Failed to connect: {ex.Message}");
            return null;
        }
    });
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();
