using System.ComponentModel;
using Docker.DotNet;
using Docker.DotNet.Models;
using ModelContextProtocol.Server;

namespace Docker_Mcp.Tools;

[McpServerToolType]
public class ImageTools
{
    private readonly Lazy<DockerClient?> _dockerClientFactory;

    public ImageTools(Lazy<DockerClient?> dockerClientFactory)
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
    [Description("Lists all Docker images")]
    public async Task<string> ListAllImagesAsync()
    {
        if (CheckDocker() is { } error) return error;
        var images = await _dockerClientFactory.Value!.Images.ListImagesAsync(
            new ImagesListParameters { All = true });

        if (images.Count == 0) return "No images were found.";

        return string.Join("\n", images.Select(i =>
            $"ID: {i.ID[..12]} | Tags: {string.Join(", ", i.RepoTags)} | " +
            $"Size: {i.Size / 1024 / 1024}MB"));
    }

    [McpServerTool]
    [Description("Pulls a Docker image from a registry")]
    public async Task<string> PullImageAsync([Description("Image name (e.g., 'nginx:latest')")] string imageName)
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            await _dockerClientFactory.Value!.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = imageName },
                null, new Progress<JSONMessage>(m => Console.WriteLine(m.ToString()))
            );

            return $"✅ Image '{imageName}' pulled successfully.";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to pull image: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Removes a Docker image by name or ID")]
    public async Task<string> RemoveImageAsync([Description("Image ID or name")] string imageName,
        [Description("Force remove image")] bool force = false)
    {
        if (CheckDocker() is { } error) return error;

        try
        {
            await _dockerClientFactory.Value!.Images.DeleteImageAsync(
                imageName, new ImageDeleteParameters { Force = force });

            return $"✅ Image '{imageName}' removed.";
        }
        catch (Exception ex)
        {
            return $"❌ Failed to remove image: {ex.Message}";
        }
    }
}