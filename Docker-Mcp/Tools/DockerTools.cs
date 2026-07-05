using System.ComponentModel;
using Docker.DotNet;
using Docker.DotNet.Models;
using ModelContextProtocol.Server;

namespace Docker_Mcp.Tools;

[McpServerToolType]
public class DockerTools
{
    private readonly DockerClient _dockerClient;

    public DockerTools(DockerClient dockerClient)
    {
        _dockerClient = dockerClient;
    }

    // ------ Containers ------

    [McpServerTool]
    [Description("List all run containers")]
    public async Task<string> ListRunnigContainersAsync()
    {
        var containers = await _dockerClient.Containers.ListContainersAsync(
            new ContainersListParameters { All = false });


        if (containers.Count == 0) return "No running container was found.";

        return string.Join("\\n", containers.Select(c =>
            $"ID: {c.ID[..12]} | Name: {c.Names[0].TrimStart('/')} | Image: {c.Image} | Status: {c.Status}"));
    }

    [McpServerTool]
    [Description("Lists all containers (running and stopped)")]
    public async Task<string> ListAllContainersAsync()
    {
        var containers = await _dockerClient.Containers.ListContainersAsync(
            new ContainersListParameters { All = true });

        if (containers.Count == 0) return "No container was found.";

        return string.Join("\n", containers.Select(c =>
            $"ID: {c.ID[..12]} | Name: {c.Names[0].TrimStart('/')} | Image: {c.Image} | State: {c.State} | Status: {c.Status}"));
    }

    [McpServerTool]
    [Description("Run a container by name")]
    public async Task<string> StartContainerAsync([Description("Id or container name")] string containerName)
    {
        await _dockerClient.Containers.StartContainerAsync(containerName, new ContainerStartParameters());
        return $"Container {containerName} started.";
    }

    [McpServerTool]
    [Description("Stop a container by name")]
    public async Task<string> StopContainerAsync([Description("Id or container name")] string containerName)
    {
        await _dockerClient.Containers.StopContainerAsync(containerName, new ContainerStopParameters());
        return $"Container {containerName} stopped.";
    }

    [McpServerTool]
    [Description("Remove a container by name")]
    public async Task<string> RemoveContainerAsync([Description("Id or container name")] string containerName)
    {
        await _dockerClient.Containers.RemoveContainerAsync(containerName,
            new ContainerRemoveParameters { Force = true });
        return $"Container {containerName} removed.";
    }

    [McpServerTool]
    [Description("Restart a container by name")]
    public async Task<string> RestartContainerAsync([Description("Id or container name")] string containerName)
    {
        await _dockerClient.Containers.RestartContainerAsync(containerName, new ContainerRestartParameters());
        return $"Container {containerName} restarted.";
    }

    [McpServerTool]
    [Description("Obtain the logs of a container by name")]
    [Obsolete("Obsolete")]
    public async Task<string> GetContainerLogsAsync([Description("Id or container name")] string containerName,
        [Description("Number of lines to show from the end of the logs")]
        int tail = 100,
        [Description("Show stdout")] bool showStdout = true, [Description("Show stderr")] bool showStderr = true)
    {
        var parameters = new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Tail = tail.ToString()
        };

        await using var stream = await _dockerClient.Containers.GetContainerLogsAsync(containerName, parameters);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}