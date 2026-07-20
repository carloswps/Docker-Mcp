using Docker_Mcp.Utils;
using Xunit;

namespace Docker_Mcp.Tests.Tools.Utils;

public class OutputFormatterTests
{
    [Theory]
    [InlineData(500, "500 B")]
    [InlineData(1_024, "1.00 KB")]
    [InlineData(1_048_576, "1.00 MB")]
    [InlineData(1_073_741_824, "1.00 GB")]
    public void FormatSize_ConvertsCorrectly(long bytes, string expected)
    {
        var result = OutputFormatter.FormatSize(bytes);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatContainer_ReturnsFormattedLine()
    {
        var result = OutputFormatter.FormatContainer("abc123", "web", "nginx", "Status: Up");

        Assert.Contains("ID: abc123", result);
        Assert.Contains("Name: web", result);
        Assert.Contains("Image: nginx", result);
        Assert.Contains("Status: Up", result);
    }

    [Fact]
    public void FormatImage_ReturnsFormattedLine()
    {
        var result = OutputFormatter.FormatImage("def456", "alpine:3.19", 7_340_032);

        Assert.Contains("ID: def456", result);
        Assert.Contains("Tags: alpine:3.19", result);
        Assert.Contains("7.00 MB", result);
    }

    [Fact]
    public void FormatVolume_ReturnsFormattedLine()
    {
        var result = OutputFormatter.FormatVolume("data", "local", "/var/lib/docker/volumes/data");

        Assert.Contains("Name: data", result);
        Assert.Contains("Driver: local", result);
        Assert.Contains("Mountpoint:", result);
    }

    [Fact]
    public void FormatNetwork_ReturnsFormattedLine()
    {
        var result = OutputFormatter.FormatNetwork("ghi789", "bridge", "bridge", "local");

        Assert.Contains("ID: ghi789", result);
        Assert.Contains("Name: bridge", result);
        Assert.Contains("Driver: bridge", result);
        Assert.Contains("Scope: local", result);
    }
}
