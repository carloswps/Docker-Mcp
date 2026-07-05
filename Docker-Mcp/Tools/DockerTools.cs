using System.ComponentModel;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Docker_Mcp.Tools;

[McpServerToolType]
public class DockerTools
{
    private readonly Lazy<DockerClient?> _dockerClientFactory;
    private readonly ILogger<DockerTools> _logger;

    public DockerTools(Lazy<DockerClient?> dockerClientFactory, ILogger<DockerTools> logger)
    {
        _dockerClientFactory = dockerClientFactory;
        _logger = logger;
    }

    private string? CheckDocker()
    {
        var client = _dockerClientFactory.Value;
        if (client is null) return "❌ Docker is not available. Make sure Docker Desktop is running.";
        return null;
    }

    // ------ Containers ------

    [McpServerTool]
    [Description("Lists all running containers")]
    public async Task<string> ListRunningContainersAsync()
    {
        if (CheckDocker() is { } error) return error;

        var containers = await _dockerClientFactory.Value!.Containers.ListContainersAsync(
            new ContainersListParameters { All = false });

        if (containers.Count == 0) return "No running containers were found.";

        return string.Join("\n", containers.Select(c =>
            $"ID: {c.ID[..12]} | Name: {c.Names[0].TrimStart('/')} | " +
            $"Image: {c.Image} | Status: {c.Status}"));
    }

    [McpServerTool]
    [Description("Lists all containers (running and stopped)")]
    public async Task<string> ListAllContainersAsync()
    {
        if (CheckDocker() is { } error) return error;

        var containers = await _dockerClientFactory.Value!.Containers.ListContainersAsync(
            new ContainersListParameters { All = true });

        if (containers.Count == 0) return "No containers were found.";

        return string.Join("\n", containers.Select(c =>
            $"ID: {c.ID[..12]} | Name: {c.Names[0].TrimStart('/')} | " +
            $"Image: {c.Image} | State: {c.State} | Status: {c.Status}"));
    }

    [McpServerTool]
    [Description("Starts a container by name or ID")]
    public async Task<string> StartContainerAsync(
        [Description("Container ID or name")] string containerName)
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            await _dockerClientFactory.Value!.Containers.StartContainerAsync(
                containerName, new ContainerStartParameters());
            return $"✅ Container '{containerName}' started.";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to start container: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Stops a container by name or ID")]
    public async Task<string> StopContainerAsync(
        [Description("Container ID or name")] string containerName)
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            await _dockerClientFactory.Value!.Containers.StopContainerAsync(
                containerName, new ContainerStopParameters());
            return $"✅ Container '{containerName}' stopped.";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to stop container: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Removes a container by name or ID")]
    public async Task<string> RemoveContainerAsync(
        [Description("Container ID or name")] string containerName,
        [Description("Force remove running container")]
        bool force = false)
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            await _dockerClientFactory.Value!.Containers.RemoveContainerAsync(
                containerName,
                new ContainerRemoveParameters { Force = force });
            return $"✅ Container '{containerName}' removed.";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to remove container: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Restarts a container by name or ID")]
    public async Task<string> RestartContainerAsync(
        [Description("Container ID or name")] string containerName)
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            await _dockerClientFactory.Value!.Containers.RestartContainerAsync(
                containerName, new ContainerRestartParameters());
            return $"✅ Container '{containerName}' restarted.";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to restart container: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets the logs of a container by name or ID")]
    public async Task<string> GetContainerLogsAsync(
        [Description("Container ID or name")] string containerName,
        [Description("Number of lines to show from the end")]
        int tail = 100)
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            var parameters = new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Tail = tail.ToString()
            };

            await using var stream = await _dockerClientFactory.Value!.Containers
                .GetContainerLogsAsync(containerName, parameters);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            return $"❌ Failed to get logs: {ex.Message}";
        }
    }

    // ------ System ------

    [McpServerTool]
    [Description("Checks if Docker is available and responsive")]
    public async Task<string> PingDockerAsync()
    {
        var client = _dockerClientFactory.Value;
        if (client is null) return "❌ Docker is not available.";

        try
        {
            await client.System.PingAsync();
            return "✅ Docker is running and responsive.";
        }
        catch (Exception ex)
        {
            return $"❌ Docker is not responding: {ex.Message}";
        }
    }
}