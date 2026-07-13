using System.ComponentModel;
using Docker.DotNet.Models;
using Docker_Mcp.Services;
using Docker_Mcp.Utils;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Docker_Mcp.Tools;

[McpServerToolType]
public class ImageTools
{
    private readonly DockerService _docker;
    private readonly ILogger<ImageTools> _logger;

    public ImageTools(DockerService docker, ILogger<ImageTools> logger)
    {
        _docker = docker;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Lists all Docker images")]
    public async Task<string> ListAllImagesAsync()
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
        {
            var images = await client.Images.ListImagesAsync(
                new ImagesListParameters { All = true });

            if (images.Count == 0) return "No images were found.";

            var lines = images.Select(i =>
                OutputFormatter.FormatImage(
                    i.ID[..12],
                    string.Join(", ", i.RepoTags),
                    i.Size));

            return string.Join("\n", lines);
        }, "list images");
    }

    [McpServerTool]
    [Description("Pulls a Docker image from a registry")]
    public async Task<string> PullImageAsync(
        [Description("Image name (e.g., 'nginx:latest')")] string imageName)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(async client =>
        {
            // Report pull progress to stderr, not stdout (stdout is the MCP transport)
            var progress = new Progress<JSONMessage>(m =>
                Console.Error.WriteLine(m.ToString()));

            await client.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = imageName },
                null, progress);

            return $"✅ Image '{imageName}' pulled successfully.";
        }, $"pull image '{imageName}'");
    }

    [McpServerTool]
    [Description("Removes a Docker image by name or ID")]
    public async Task<string> RemoveImageAsync(
        [Description("Image ID or name")] string imageName,
        [Description("Force remove image")] bool force = false)
    {
        if (_docker.CheckDocker() is { } error) return error;

        return await _docker.TryExecuteAsync(
            client => client.Images.DeleteImageAsync(
                imageName, new ImageDeleteParameters { Force = force }),
            $"✅ Image '{imageName}' removed.",
            $"remove image '{imageName}'");
    }
}
