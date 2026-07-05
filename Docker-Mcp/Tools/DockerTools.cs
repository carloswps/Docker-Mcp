using ModelContextProtocol.Server;

namespace Docker_Mcp.Tools;

public class DockerTools
{
    [McpServerTool(Name = "list_containers", Description = "Lists all active or all available Docker containers on the host.")]
    public async Task<string> ListContainersAsync(bool all = false)
    {
    }
}
