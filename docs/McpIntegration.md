# MCP Integration for AI Agents

ChangeSharp provides a built-in Model Context Protocol (MCP) server, allowing AI agents (like Claude, Cursor, or GitHub Copilot) to interact directly with your changelog and release workflow.

## Features

The MCP server exposes the following tools:

- `get_status`: Get the count of pending fragments, the current version, and the next calculated version.
- `create_fragment`: Create a new change fragment with a message and a category.
- `validate_fragments`: Ensure all pending fragments follow the correct format.
- `perform_release`: Execute a release, aggregate changes into the changelog, and bump project versions.

## Configuration

To use the ChangeSharp MCP server, you need to add it to your AI agent's configuration.

### Claude Desktop

Add the following to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "changesharp": {
      "command": "dotnet",
      "args": ["run", "--project", "/absolute/path/to/ChangeSharp.Mcp/ChangeSharp.Mcp.csproj"]
    }
  }
}
```

### Cursor

1. Open Cursor Settings.
2. Go to **Features** > **MCP**.
3. Click **Add New MCP Server**.
4. Name: `ChangeSharp`
5. Type: `command`
6. Command: `dotnet run --project /absolute/path/to/ChangeSharp.Mcp/ChangeSharp.Mcp.csproj`

## Benefits

By integrating ChangeSharp with your AI agent:
- The agent can automatically create fragments as it implements new features or fixes bugs.
- You can ask the agent: "What's the status of our next release?"
- The agent can help you prepare a release by summarizing the changes.
- Validation is automated before any release is performed.
