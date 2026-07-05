# Docker-Mcp

An MCP (Model Context Protocol) server that exposes Docker operations as tools, so any MCP-compatible client (Claude Desktop, OpenCode, etc.) can manage your Docker containers, images and volumes through natural language.

Written in C# / .NET 10, built on top of [Docker.DotNet](https://github.com/dotnet/Docker.DotNet) and the official [ModelContextProtocol](https://github.com/modelcontextprotocol/csharp-sdk) C# SDK.

## Features

The server speaks MCP over **stdio** and auto-registers every tool decorated with `[McpServerTool]` in the assembly. Currently exposed tools:

### Containers
| Tool | Description |
|------|-------------|
| `ListRunningContainers` | Lists all running containers |
| `ListAllContainers` | Lists all containers (running and stopped) |
| `StartContainer` | Starts a container by name or ID |
| `StopContainer` | Stops a container by name or ID |
| `RemoveContainer` | Removes a container (optional `force`) |
| `RestartContainer` | Restarts a container by name or ID |
| `GetContainerLogs` | Returns the container logs (configurable `tail`) |

### Images
| Tool | Description |
|------|-------------|
| `ListAllImages` | Lists all Docker images |
| `PullImage` | Pulls an image from a registry |
| `RemoveImage` | Removes an image (optional `force`) |

### Volumes
| Tool | Description |
|------|-------------|
| `ListVolumes` | Lists all Docker volumes |
| `CreateVolume` | Creates a new volume (optional `driver`) |
| `RemoveVolume` | Removes a volume by name (optional `force`) |

### System
| Tool | Description |
|------|-------------|
| `PingDocker` | Checks if the Docker daemon is available and responsive |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A working Docker daemon on the host
  - Windows: `npipe://./pipe/docker_engine` (Docker Desktop)
  - Linux / macOS: `unix:///var/run/docker.sock`

## Build

```bash
dotnet restore
dotnet build
```

Release build (matches CI, treats warnings as errors):

```bash
dotnet build --configuration Release --warnaserror
```

## Run

The server is meant to be launched by an MCP client, not run interactively:

```bash
dotnet run --project Docker-Mcp/Docker-Mcp.csproj
```

## Configure with an MCP client

Add the server to your MCP client config. Example for OpenCode / Claude Desktop:

```json
{
  "mcpServers": {
    "docker-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/path/to/Docker-Mcp/Docker-Mcp/Docker-Mcp.csproj"]
    }
  }
}
```

Adjust the path to your local checkout.

> Logs are emitted to **stderr**, so they never corrupt the MCP stream on **stdout**. Tool errors are returned as human-readable `❌ ...` strings instead of being thrown.

## Project structure

```
Docker-Mcp/
├── Docker-Mcp.sln
└── Docker-Mcp/
    ├── Program.cs          # Host setup, Docker client registration, MCP server wiring
    └── Tools/
        ├── DockerTools.cs     # Container + system tools
        ├── ImageTools.cs      # Image tools
        └── VolumeTools.cs     # Volume tools
```

New tools are auto-discovered — just add a class annotated with `[McpServerToolType]` and methods annotated with `[McpServerTool]` in `Tools/`. No manual DI registration required.

## CI

`.github/workflows/ci.yml` builds the project on `ubuntu-latest` and `windows-latest` with .NET `10.0.x`, treating warnings as errors.

## License

See the repository for license details.