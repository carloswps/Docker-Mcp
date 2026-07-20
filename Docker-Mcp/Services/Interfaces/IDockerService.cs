using Docker.DotNet;

namespace Docker_Mcp.Services.Interfaces;

public interface IDockerService
{
    string? CheckDocker();
    Task<string> TryExecuteAsync(Func<DockerClient, Task<string>> operation, string? errorContext = null);
    Task<string> TryExecuteAsync(Func<DockerClient, Task> operation, string successMessage, string? errorContext = null);
}
