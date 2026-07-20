using Docker.DotNet;
using Docker_Mcp.Services.Interfaces;
using Docker_Mcp.Tools;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Docker_Mcp.Tests.Tools;

public class DockerToolsTests
{
    private readonly Mock<IDockerService> _dockerMock;
    private readonly Mock<ILogger<DockerTools>> _loggerMock;
    private readonly DockerTools _sut;

    public DockerToolsTests()
    {
        _dockerMock = new Mock<IDockerService>();
        _loggerMock = new Mock<ILogger<DockerTools>>();
        _sut = new DockerTools(_dockerMock.Object, _loggerMock.Object);
    }

    // ── Docker unavailable ──

    [Fact]
    public async Task ListRunningContainers_WhenDockerUnavailable_ReturnsError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns("❌ Docker is not available.");

        var result = await _sut.ListRunningContainersAsync();

        Assert.StartsWith("❌", result);
    }

    [Fact]
    public async Task StartContainer_WhenDockerUnavailable_ReturnsError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns("❌ Docker is not available.");

        var result = await _sut.StartContainerAsync("my-container");

        Assert.StartsWith("❌", result);
    }

    // ── Success ──

    [Fact]
    public async Task StartContainer_WhenAvailable_ReturnsSuccess()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Container 'my-container' started.");

        var result = await _sut.StartContainerAsync("my-container");

        Assert.Contains("✅", result);
        Assert.Contains("my-container", result);
    }

    [Fact]
    public async Task StopContainer_WhenAvailable_ReturnsSuccess()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Container 'web-app' stopped.");

        var result = await _sut.StopContainerAsync("web-app");

        Assert.Contains("✅", result);
        Assert.Contains("stopped", result);
    }

    // ── Failure ──

    [Fact]
    public async Task RemoveContainer_WhenOperationFails_ReturnsFormattedError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("❌ Operation failed (remove container 'ghost'): container not found");

        var result = await _sut.RemoveContainerAsync("ghost");

        Assert.Contains("❌", result);
        Assert.Contains("ghost", result);
    }

    // ── Ping ──

    [Fact]
    public async Task PingDocker_WhenAvailable_ReturnsRunningMessage()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Docker is running and responsive.");

        var result = await _sut.PingDockerAsync();

        Assert.Contains("✅", result);
        Assert.Contains("running", result);
    }

    [Fact]
    public async Task PingDocker_WhenUnavailable_ReturnsError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns("❌ Docker is not available.");

        var result = await _sut.PingDockerAsync();

        Assert.Contains("❌", result);
        Assert.DoesNotContain("running", result);
    }
}
