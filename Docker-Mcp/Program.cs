using System.Runtime.InteropServices;
using Docker.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleOptions => { consoleOptions.LogToStandardErrorThreshold = LogLevel.Warning; });


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

    return new DockerClientConfiguration(dockerUri).CreateClient();
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();