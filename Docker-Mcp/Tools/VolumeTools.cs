using System.ComponentModel;
using Docker.DotNet.Models;
using Docker_Mcp.Services;
using Docker_Mcp.Services.Interfaces;
using Docker_Mcp.Utils;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Docker_Mcp.Tools;

[McpServerToolType]
public class VolumeTools
{
    private readonly IDockerService _docker;
    private readonly ILogger<VolumeTools> _logger;

    public VolumeTools(IDockerService docker, ILogger<VolumeTools> logger)
    {
        _docker = docker;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Lists all Docker volumes")]
    public async Task<string> ListVolumesAsync()
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
        {
            var volumes = await client.Volumes.ListAsync(
                new VolumesListParameters());

            if (volumes.Volumes.Count == 0) return "No volumes found.";

            var lines = volumes.Volumes.Select(v =>
                OutputFormatter.FormatVolume(v.Name, v.Driver, v.Mountpoint));

            return string.Join("\n", lines);
        }, "list volumes");
    }

    [McpServerTool]
    [Description("Creates a new Docker volume")]
    public async Task<string> CreateVolumeAsync(
        [Description("Volume name")] string name,
        [Description("Volume driver (default: local)")] string driver = "local")
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Volumes.CreateAsync(
                new VolumesCreateParameters { Name = name, Driver = driver }),
            $"✅ Volume '{name}' created.",
            $"create volume '{name}'");
    }

    [McpServerTool]
    [Description("Removes a Docker volume by name")]
    public async Task<string> RemoveVolumeAsync(
        [Description("Volume name")] string name,
        [Description("Force remove")] bool force = false)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Volumes.RemoveAsync(name, force),
            $"✅ Volume '{name}' removed.",
            $"remove volume '{name}'");
    }
}
