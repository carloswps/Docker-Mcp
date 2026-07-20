using Docker.DotNet;
using Docker_Mcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Docker_Mcp.Services;

public class DockerService : IDockerService
{
    private readonly Lazy<DockerClient?> _dockerClientFactory;
    private readonly ILogger<DockerService> _logger;

    public DockerService(Lazy<DockerClient?> dockerClientFactory, ILogger<DockerService> logger)
    {
        _dockerClientFactory = dockerClientFactory;
        _logger = logger;
    }

    public string? CheckDocker()
    {
        if (_dockerClientFactory.Value is null)
        {
            _logger.LogWarning("Docker client is not available — daemon may not be running.");
            return "❌ Docker is not available. Make sure Docker Desktop is running";
        }

        return null;
    }

    public DockerClient Client => _dockerClientFactory.Value!;


    public async Task<string> TryExecuteAsync(
        Func<DockerClient, Task<string>> operation, string? errorContext = null
    )
    {
        try
        {
            return await operation(Client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorContext ?? "An error occurred while executing Docker operation.");
            return $"❌ Operation failed ({errorContext}): {ex.Message}";
        }
    }

    public async Task<string> TryExecuteAsync(
        Func<DockerClient, Task> operation, string successMessage,
        string? errorContext = null
    )
    {
        try
        {
            await operation(Client);
            return successMessage ?? "✅ Operation completed successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorContext ?? "An error occurred while executing Docker operation.");
            return $"❌ Operation failed ({errorContext}): {ex.Message}";
        }
    }
}
