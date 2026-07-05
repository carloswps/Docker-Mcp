using System.ComponentModel;
using Docker.DotNet;
using Docker.DotNet.Models;
using ModelContextProtocol.Server;

namespace Docker_Mcp.Tools;

[McpServerToolType]
public class VolumeTools
{
    private readonly Lazy<DockerClient?> _dockerClientFactory;

    public VolumeTools(Lazy<DockerClient?> dockerClientFactory)
    {
        _dockerClientFactory = dockerClientFactory;
    }

    private string? CheckDocker()
    {
        return _dockerClientFactory.Value is null
            ? "❌ Docker is not available."
            : null;
    }

    [McpServerTool]
    [Description("Lists all Docker volumes")]
    public async Task<string> ListVolumesAsync()
    {
        if (CheckDocker() is { } error) return error;

        var volumes = await _dockerClientFactory.Value!.Volumes.ListAsync(
            new VolumesListParameters());

        if (volumes.Volumes.Count == 0) return "No volumes found.";

        return string.Join("\n", volumes.Volumes.Select(v =>
            $"Name: {v.Name} | Driver: {v.Driver} | Mountpoint: {v.Mountpoint}"));
    }

    [McpServerTool]
    [Description("Creates a new Docker volume")]
    public async Task<string> CreateVolumeAsync(
        [Description("Volume name")] string name,
        [Description("Volume driver (default: local)")]
        string driver = "local")
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            await _dockerClientFactory.Value!.Volumes.CreateAsync(
                new VolumesCreateParameters { Name = name, Driver = driver });

            return $"✅ Volume '{name}' created.";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to create volume: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Removes a Docker volume by name")]
    public async Task<string> RemoveVolumeAsync(
        [Description("Volume name")] string name,
        [Description("Force remove")] bool force = false)
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            await _dockerClientFactory.Value!.Volumes.RemoveAsync(
                name, force);

            return $"✅ Volume '{name}' removed.";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to remove volume: {ex.Message}";
        }
    }
}