using System.ComponentModel;
using Docker.DotNet.Models;
using Docker_Mcp.Services;
using Docker_Mcp.Utils;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Docker_Mcp.Tools;

[McpServerToolType]
public class DockerTools
{
    private readonly DockerService _docker;
    private readonly ILogger<DockerTools> _logger;

    public DockerTools(DockerService docker, ILogger<DockerTools> logger)
    {
        _docker = docker;
        _logger = logger;
    }

    // ── Containers ──

    [McpServerTool]
    [Description("Lists all running containers")]
    public async Task<string> ListRunningContainersAsync()
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
        {
            var containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters { All = false });

            if (containers.Count == 0) return "No running containers were found.";

            var lines = containers.Select(c =>
                OutputFormatter.FormatContainer(
                    c.ID[..12],
                    c.Names[0].TrimStart('/'),
                    c.Image,
                    $"Status: {c.Status}"));

            return string.Join("\n", lines);
        }, "list running containers");
    }

    [McpServerTool]
    [Description("Lists all containers (running and stopped)")]
    public async Task<string> ListAllContainersAsync()
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
        {
            var containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters { All = true });

            if (containers.Count == 0) return "No containers were found.";

            var lines = containers.Select(c =>
                OutputFormatter.FormatContainer(
                    c.ID[..12],
                    c.Names[0].TrimStart('/'),
                    c.Image,
                    $"State: {c.State} | Status: {c.Status}"));

            return string.Join("\n", lines);
        }, "list all containers");
    }

    [McpServerTool]
    [Description("Starts a container by name or ID")]
    public async Task<string> StartContainerAsync(
        [Description("Container ID or name")] string containerName)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Containers.StartContainerAsync(
                containerName, new ContainerStartParameters()),
            $"✅ Container '{containerName}' started.",
            $"start container '{containerName}'");
    }

    [McpServerTool]
    [Description("Stops a container by name or ID")]
    public async Task<string> StopContainerAsync(
        [Description("Container ID or name")] string containerName)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Containers.StopContainerAsync(
                containerName, new ContainerStopParameters()),
            $"✅ Container '{containerName}' stopped.",
            $"stop container '{containerName}'");
    }

    [McpServerTool]
    [Description("Removes a container by name or ID")]
    public async Task<string> RemoveContainerAsync(
        [Description("Container ID or name")] string containerName,
        [Description("Force remove running container")] bool force = false)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Containers.RemoveContainerAsync(
                containerName, new ContainerRemoveParameters { Force = force }),
            $"✅ Container '{containerName}' removed.",
            $"remove container '{containerName}'");
    }

    [McpServerTool]
    [Description("Restarts a container by name or ID")]
    public async Task<string> RestartContainerAsync(
        [Description("Container ID or name")] string containerName)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Containers.RestartContainerAsync(
                containerName, new ContainerRestartParameters()),
            $"✅ Container '{containerName}' restarted.",
            $"restart container '{containerName}'");
    }

    [McpServerTool]
    [Description("Gets the logs of a container by name or ID")]
    public async Task<string> GetContainerLogsAsync(
        [Description("Container ID or name")] string containerName,
        [Description("Number of lines to show from the end")] int tail = 100)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
        {
            var parameters = new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Tail = tail.ToString()
            };

            using var stream = await client.Containers
                .GetContainerLogsAsync(containerName, false, parameters);

            using var stdout = new MemoryStream();
            using var stderr = new MemoryStream();

            await stream.CopyOutputToAsync(Stream.Null, stdout, stderr, CancellationToken.None);

            stdout.Position = 0;

            using var reader = new StreamReader(stdout);
            return await reader.ReadToEndAsync();
        }, $"get logs for '{containerName}'");
    }

    // ── System ──

    [McpServerTool]
    [Description("Checks if Docker is available and responsive")]
    public async Task<string> PingDockerAsync()
    {
        var error = _docker.CheckDocker();
        if (error is not null) return error;

        return await _docker.TryExecuteAsync(
            client => client.System.PingAsync(),
            "✅ Docker is running and responsive.",
            "ping");
    }
}
