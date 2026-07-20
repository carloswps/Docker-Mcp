using Docker.DotNet;
using Docker_Mcp.Services.Interfaces;
using Docker_Mcp.Tools;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Docker_Mcp.Tests.Tools;

public class ImageToolsTests
{
    private readonly Mock<IDockerService> _dockerMock;
    private readonly Mock<ILogger<ImageTools>> _loggerMock;
    private readonly ImageTools _sut;

    public ImageToolsTests()
    {
        _dockerMock = new Mock<IDockerService>();
        _loggerMock = new Mock<ILogger<ImageTools>>();
        _sut = new ImageTools(_dockerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ListAllImages_WhenDockerUnavailable_ReturnsError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns("❌ Docker is not available.");

        var result = await _sut.ListAllImagesAsync();

        Assert.StartsWith("❌", result);
    }

    [Fact]
    public async Task ListAllImages_WhenNoImages_ReturnsEmptyMessage()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task<string>>>(),
                It.IsAny<string?>()))
            .ReturnsAsync("No images were found.");

        var result = await _sut.ListAllImagesAsync();

        Assert.Equal("No images were found.", result);
    }

    [Fact]
    public async Task PullImage_WhenAvailable_ReturnsSuccess()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task<string>>>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Image 'nginx:latest' pulled successfully.");

        var result = await _sut.PullImageAsync("nginx:latest");

        Assert.Contains("✅", result);
        Assert.Contains("nginx:latest", result);
    }

    [Fact]
    public async Task RemoveImage_WhenOperationFails_ReturnsError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("❌ Operation failed (remove image 'bad-image'): image in use");

        var result = await _sut.RemoveImageAsync("bad-image");

        Assert.Contains("❌", result);
        Assert.Contains("bad-image", result);
    }

    [Fact]
    public async Task RemoveImage_ForceRemove_IsAllowed()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Image 'stubborn' removed.");

        var result = await _sut.RemoveImageAsync("stubborn", force: true);

        Assert.Contains("✅", result);
    }
}
