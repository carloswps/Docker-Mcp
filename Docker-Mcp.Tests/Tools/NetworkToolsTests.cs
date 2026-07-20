using Docker.DotNet;
using Docker_Mcp.Services.Interfaces;
using Docker_Mcp.Tools;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Docker_Mcp.Tests.Tools;

public class NetworkToolsTests
{
    private readonly Mock<IDockerService> _dockerMock;
    private readonly Mock<ILogger<NetworkTools>> _loggerMock;
    private readonly NetworkTools _sut;

    public NetworkToolsTests()
    {
        _dockerMock = new Mock<IDockerService>();
        _loggerMock = new Mock<ILogger<NetworkTools>>();
        _sut = new NetworkTools(_loggerMock.Object, _dockerMock.Object);
    }

    [Fact]
    public async Task ListNetwork_WhenDockerUnavailable_ReturnsError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns("❌ Docker is not available.");

        var result = await _sut.ListNetworkAsync();

        Assert.StartsWith("❌", result);
    }

    [Fact]
    public async Task CreateNetwork_WhenAvailable_ReturnsSuccess()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Network 'my-net' created with driver 'bridge'.");

        var result = await _sut.CreateNetworkAsync("my-net");

        Assert.Contains("✅", result);
        Assert.Contains("my-net", result);
    }

    [Fact]
    public async Task CreateNetwork_WithOverlayDriver_PassesThrough()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Network 'swarm-net' created with driver 'overlay'.");

        var result = await _sut.CreateNetworkAsync("swarm-net", driver: "overlay");

        Assert.Contains("✅", result);
        Assert.Contains("overlay", result);
    }

    [Fact]
    public async Task ConnectNetwork_WhenAvailable_ReturnsSuccess()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Container 'app' connected to network 'bridge-net'.");

        var result = await _sut.ConnectNetworkAsync("bridge-net", "app");

        Assert.Contains("✅", result);
        Assert.Contains("app", result);
        Assert.Contains("bridge-net", result);
    }

    [Fact]
    public async Task DisconnectNetwork_Force_IsAllowed()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Container 'stuck-app' disconnected from network 'prod-net'.");

        var result = await _sut.DisconnectNetworkAsync("prod-net", "stuck-app", force: true);

        Assert.Contains("✅", result);
    }

    [Fact]
    public async Task PruneNetworks_WhenNoUnused_ReturnsMessage()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task<string>>>(),
                It.IsAny<string?>()))
            .ReturnsAsync("No unused networks to prune.");

        var result = await _sut.PruneNetworksAsync();

        Assert.Equal("No unused networks to prune.", result);
    }
}
