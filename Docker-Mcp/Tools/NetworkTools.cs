using System.ComponentModel;
using Docker_Mcp.Services;
using Docker_Mcp.Utils;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Docker_Mcp.Services.Interfaces;

namespace Docker_Mcp.Tools;

[McpServerToolType]
public class NetworkTools
{
    private readonly IDockerService _docker;
    private readonly ILogger<NetworkTools> _logger;

    public NetworkTools(ILogger<NetworkTools> logger, IDockerService docker)
    {
        _logger = logger;
        _docker = docker;
    }

    [McpServerTool]
    [Description("List all Docker networks")]
    public async Task<string> ListNetworkAsync()
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
        {
            var networks = await client.Networks.ListNetworksAsync();

            if (networks.Count == 0) return "No networks found.";

            var lines = networks.Select(n => OutputFormatter.FormatNetwork(
                n.ID[..12],
                n.Name,
                n.Driver,
                n.Scope));
            return string.Join("\n", lines);
        }, "list networks");
    }

    [McpServerTool]
    [Description("Creates a new Docker network")]
    public async Task<string> CreateNetworkAsync(
        [Description("Network name")] string name,
        [Description("Network driver (e.g., bridge, overlay)")]
        string driver = "bridge")
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Networks.CreateNetworkAsync(
                new NetworksCreateParameters
                {
                    Name = name,
                    Driver = driver,
                    CheckDuplicate = true
                }
            ),
            $"✅ Network '{name}' created with driver '{driver}'.",
            $"create network '{name}'");
    }

    [McpServerTool]
    [Description("Removes a Docker network by name or ID")]
    public async Task<string> RemoveNetworkAsync(
        [Description("Network name or Id")] string name
    )
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(client => client.Networks.DeleteNetworkAsync(name),
            $"✅ Network '{name}' removed.",
            $"remove network '{name}'");
    }

    [McpServerTool]
    [Description("Inspects a Docker network by name or ID")]
    public async Task<string> InspectNetworkAsync(
        [Description("Network name or ID")] string name)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
            {
                var network = await client.Networks.InspectNetworkAsync(name);

                var containers = network.Containers?.Count > 0
                    ? string.Join(", ", network.Containers.Values.Select(c => c.Name))
                    : "none";

                return string.Join("\n",
                    $"Name: {network.Name}",
                    $"ID: {network.ID}",
                    $"Driver: {network.Driver}",
                    $"Scope: {network.Scope}",
                    $"Subnet: {network.IPAM?.Config?.FirstOrDefault()?.Subnet ?? "N/A"}",
                    $"Gateway: {network.IPAM?.Config?.FirstOrDefault()?.Gateway ?? "N/A"}",
                    $"Containers: {containers}");
            }, $"inspect network '{name}'");
    }

    [McpServerTool]
    [Description("Connects a container to a Docker network")]
    public async Task<string> ConnectNetworkAsync(
        [Description("Network name or ID")] string networkName,
        [Description("Container name or ID")] string containerName)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Networks.ConnectNetworkAsync(
                networkName,
                new NetworkConnectParameters { Container = containerName }),
            $"✅ Container '{containerName}' connected to network '{networkName}'.",
            $"connect container '{containerName}' to network '{networkName}'");
    }

    [McpServerTool]
    [Description("Disconnects a container from a Docker network")]
    public async Task<string> DisconnectNetworkAsync(
        [Description("Network name or ID")] string networkName,
        [Description("Container name or ID")] string containerName,
        [Description("Force disconnect")] bool force = false)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Networks.DisconnectNetworkAsync(
                networkName,
                new NetworkDisconnectParameters { Container = containerName, Force = force }),
            $"✅ Container '{containerName}' disconnected from network '{networkName}'.",
            $"disconnect container '{containerName}' from network '{networkName}'");
    }

    [McpServerTool]
    [Description("Prunes unused Docker networks")]
    public async Task<string> PruneNetworksAsync()
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
        {
            var result = await client.Networks.PruneNetworksAsync();

            if (result.NetworksDeleted is null || result.NetworksDeleted.Count == 0)
                return "No unused networks to prune.";

            var deleted = string.Join(", ", result.NetworksDeleted);
            return $"✅ Pruned networks: {deleted}";
        }, "prune networks");
    }
}
