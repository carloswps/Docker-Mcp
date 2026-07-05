using System.Runtime.InteropServices;
using Docker.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleOptions => { consoleOptions.LogToStandardErrorThreshold = LogLevel.Information; });


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