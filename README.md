# 🐳 Docker MCP Server

A **Model Context Protocol (MCP)** server built with **C#** and **.NET** that enables AI assistants to interact with the Docker Engine through natural language.

This project exposes Docker operations as MCP tools, allowing clients such as Claude Desktop, Cline, VS Code, Zed, OpenCode, and other MCP-compatible applications to manage Docker containers safely and efficiently.

> **⚠️ Educational Project**
>
> This repository is primarily an educational project created to study and explore:
>
> - Model Context Protocol (MCP)
> - C# and modern .NET development
> - Docker Engine API integration
> - Software architecture and design patterns
> - AI tooling and automation
>
> While the project aims to follow production-quality coding practices, its main goal is learning, experimentation, and continuous improvement.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![Docker](https://img.shields.io/badge/Docker-Required-2496ED)
![License](https://img.shields.io/badge/license-MIT-green)

---

# ✨ Features

Current features include:

- 🐳 Container management
  - List running containers
  - List all containers (including stopped)
  - Start containers
  - Stop containers
  - Restart containers
  - Remove containers
  - View logs with tail support

- 💚 Docker daemon health check

- 🖼️ Image management
  - List images
  - Pull images
  - Remove images

- 💾 Volume management
  - List volumes
  - Create volumes
  - Remove volumes

- 🚀 Self-contained executable

- 🔒 Friendly error handling

---

# 🚧 Roadmap

## Version 0.1

- [x] Docker connection
- [x] Health check
- [x] List containers
- [x] Start container
- [x] Stop container
- [x] Restart container
- [x] View logs

## Version 0.2

- [ ] Docker Compose support

## Version 0.3

- [x] Image management
  - [x] List images
  - [x] Pull images
  - [x] Remove images

## Version 0.4

- [ ] Network management

## Version 0.5

- [x] Volume management
  - [x] List volumes
  - [x] Create volumes
  - [x] Remove volumes

## Version 1.0

- [ ] Container diagnostics
- [ ] Resource monitoring
- [ ] Smarter Docker assistant tools

---

# 📦 Installation

## Option 1 — Download Release (Recommended)

Download the latest release from the **Releases** page.

```bash
chmod +x Docker-Mcp
./Docker-Mcp
```

---

## Option 2 — Build from Source

Requirements:

- .NET 10 SDK
- Docker Engine or Docker Desktop

```bash
git clone https://github.com/your-user/docker-mcp.git

cd docker-mcp/Docker-Mcp

dotnet publish \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -p:PublishSingleFile=true \
    -o ./publish

./publish/Docker-Mcp
```

---

# ⚙️ Configuration

## Claude Desktop

Add the following configuration to:

### Linux

```
~/.config/Claude/claude_desktop_config.json
```

### macOS

```
~/Library/Application Support/Claude/claude_desktop_config.json
```

### Windows

```
%APPDATA%\Claude\claude_desktop_config.json
```

```json
{
  "mcpServers": {
    "docker": {
      "command": "/absolute/path/to/Docker-Mcp"
    }
  }
}
```

Restart Claude Desktop.

The Docker tools will become available automatically.

---

## MCP Inspector

Useful during development.

```bash
npx @modelcontextprotocol/inspector /absolute/path/to/Docker-Mcp
```

---

# 🛠️ Available Tools

| Tool | Description |
|------|-------------|
| `PingDockerAsync` | Verify Docker daemon availability |
| `ListRunningContainersAsync` | List running containers |
| `ListAllContainersAsync` | List every container |
| `StartContainerAsync` | Start a container |
| `StopContainerAsync` | Stop a container |
| `RestartContainerAsync` | Restart a container |
| `RemoveContainerAsync` | Remove a container |
| `GetContainerLogsAsync` | Retrieve container logs |
| `ListAllImagesAsync` | List all Docker images |
| `PullImageAsync` | Pull an image from a registry |
| `RemoveImageAsync` | Remove an image |
| `ListVolumesAsync` | List all Docker volumes |
| `CreateVolumeAsync` | Create a new volume |
| `RemoveVolumeAsync` | Remove a volume |

---

# 💬 Example Prompts

Examples of prompts you can send to your AI assistant:

> Is Docker running?

> Show me all running containers.

> List all containers, including stopped ones.

> Restart the nginx container.

> Show the last 100 log lines from my PostgreSQL container.

> Remove the old Redis container.

---

# 🐳 Docker Requirements

- Docker Engine 20.10+
- Docker Desktop (Windows/macOS)

### Linux

Your user must belong to the **docker** group in order to access:

```
/var/run/docker.sock
```

---

# 🛠️ Development

Restore packages

```bash
dotnet restore
```

Build

```bash
dotnet build
```

Run

```bash
dotnet run --project Docker-Mcp/Docker-Mcp.csproj
```

Publish

```bash
dotnet publish \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -p:PublishSingleFile=true
```

---

# 🤝 Contributing

Contributions, suggestions, and feedback are welcome.

Since this project is part of my learning journey, improvements and discussions are greatly appreciated.


---

Built using C#, .NET and Docker.