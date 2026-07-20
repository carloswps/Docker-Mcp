using Docker.DotNet;
using Docker_Mcp.Services.Interfaces;
using Docker_Mcp.Tools;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Docker_Mcp.Tests.Tools;

public class VolumeToolsTests
{
    private readonly Mock<IDockerService> _dockerMock;
    private readonly Mock<ILogger<VolumeTools>> _loggerMock;
    private readonly VolumeTools _sut;

    public VolumeToolsTests()
    {
        _dockerMock = new Mock<IDockerService>();
        _loggerMock = new Mock<ILogger<VolumeTools>>();
        _sut = new VolumeTools(_dockerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ListVolumes_WhenDockerUnavailable_ReturnsError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns("❌ Docker is not available.");

        var result = await _sut.ListVolumesAsync();

        Assert.StartsWith("❌", result);
    }

    [Fact]
    public async Task CreateVolume_WhenAvailable_ReturnsSuccess()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Volume 'my-data' created.");

        var result = await _sut.CreateVolumeAsync("my-data");

        Assert.Contains("✅", result);
        Assert.Contains("my-data", result);
    }

    [Fact]
    public async Task CreateVolume_WithCustomDriver_PassesThrough()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Volume 'nfs-vol' created.");

        var result = await _sut.CreateVolumeAsync("nfs-vol", driver: "nfs");

        Assert.Contains("✅", result);
    }

    [Fact]
    public async Task RemoveVolume_ForceRemove_IsAllowed()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("✅ Volume 'old-data' removed.");

        var result = await _sut.RemoveVolumeAsync("old-data", force: true);

        Assert.Contains("✅", result);
    }

    [Fact]
    public async Task RemoveVolume_WhenOperationFails_ReturnsError()
    {
        _dockerMock.Setup(d => d.CheckDocker()).Returns((string?)null);
        _dockerMock
            .Setup(d => d.TryExecuteAsync(
                It.IsAny<Func<DockerClient, Task>>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .ReturnsAsync("❌ Operation failed (remove volume 'in-use'): volume is in use");

        var result = await _sut.RemoveVolumeAsync("in-use");

        Assert.Contains("❌", result);
    }
}
