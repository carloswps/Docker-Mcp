# AGENTS.md

Guidance for AI agents working in this repository.

## Project

Single-project .NET 10 solution exposing Docker operations as an MCP (Model Context Protocol) server over stdio. The server (`Docker-Mcp/Docker-Mcp.csproj`) is the only project in `Docker-Mcp.sln`.

## Commands

- **Build / typecheck:** `dotnet build` (C# compile doubles as typecheck; no separate typecheck step)
- **Release build (matches CI):** `dotnet build --configuration Release --warnaserror`
- **Restore:** `dotnet restore`
- **Run:** `dotnet run --project Docker-Mcp/Docker-Mcp.csproj` — requires a working Docker daemon on the host; the server speaks MCP over stdio and is meant to be launched by an MCP client, not run interactively.
- **Tests:** none. There is no test project and no test runner.

CI (`.github/workflows/ci.yml`) runs only `dotnet build --no-restore --configuration Release --warnaserror` on `ubuntu-latest` and `windows-latest` with .NET `10.0.x`. Treat warnings as errors locally before committing.

## Architecture

- `Program.cs` is the only entrypoint. It builds a generic host, registers a `Lazy<DockerClient?>` singleton, and calls `AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`.
- MCP tools are auto-discovered from the assembly: any public class annotated `[McpServerToolType]` with methods annotated `[McpServerTool]` is registered. Add a new tool class in `Tools/` with those attributes — no manual DI registration needed.
- Docker client URI is platform-dependent: `npipe://./pipe/docker_engine` on Windows, `unix:///var/run/docker.sock` on Linux/macOS. The `Lazy<>` swallows connection failures and returns `null`; every tool guards with a `CheckDocker()` helper and returns an error string rather than throwing.
- Console logging is redirected to **stderr** (`LogToStandardErrorThreshold = Information`) because **stdout is the MCP transport** — never `Console.WriteLine` to stdout in tools (it corrupts the protocol). Existing `Console.WriteLine` in `ImageTools.PullImageAsync` is a latent bug.
- Tool methods return `string` (human-readable, emoji-prefixed results) rather than throwing. Preserve that convention: catch exceptions inside the tool and return a `❌ ...` message.

## Docker.DotNet API quirks (version 3.125.15)

Method names on `IVolumeOperations` dropped the `Volume` suffix used in older docs:

- `Volumes.ListAsync(VolumesListParameters)` — not `ListVolumesAsync`
- `Volumes.CreateAsync(VolumesCreateParameters)` — not `CreateVolumeAsync`
- `Volumes.RemoveAsync(name, force)` — not `DeleteVolumeAsync`

`IContainerOperations` and `IImageOperations` keep the older `*ContainerAsync` / `*ImageAsync` / `DeleteImageAsync` names. When in doubt, reflect `IVolumeOperations` / `IContainerOperations` / `IImageOperations` from `Docker.DotNet.dll` rather than guessing — this package's API has changed across versions and old samples online will mislead you.

`IContainerOperations.GetContainerLogsAsync(string, ContainerLogsParameters, CancellationToken)` is `[Obsolete]`; use the overload with the `bool` parameter if you touch `DockerTools.GetContainerLogsAsync`.

## Conventions

- Namespace is `Docker_Mcp.Tools` / `Docker_Mcp.Services` / etc. (underscore, not dash) despite the project name `Docker-Mcp`.
- Empty `Models/`, `Services/`, `Utils/` folders are kept with `.gitkeep`; do not delete them.
- Tool method parameters use `[Description("...")]` from `System.ComponentModel` — MCP exposes these as the schema description for the client. Keep them concise and user-facing.