# Architecture

ChangeSharp follows a layered architecture to ensure separation of concerns, testability, and extensibility.

## Core Layers

### 1. ChangeSharp.Core (Domain & Business Logic)
The core library contains all the logic for parsing, merging, and version calculation. It is independent of any interface (CLI or MCP).
- `ChangeLog.cs`: Represents the global changelog file and handles insertion operations (updating *Unreleased* and generating a release).
- `ChangeSet.cs`: Models the changes categorized by section.
- `ChangelogParser.cs`: Reconstructs a `ChangeSet` from Markdown input using **Markdig**.
- `ChangeSetMerger.cs`: Merges multiple `ChangeSet` instances.
- `NextVersionComputer.cs`: Computes the next version based on populated sections.
- `WorkspaceManager.cs`: Orchestrates the filesystem operations, configuration, and the overall release workflow.
- `VersionPropagation/`: Contains handlers for updating version numbers in various file types (MSBuild, JSON, Regex).

### 2. ChangeSharp.CLI (The Source of Truth)
The CLI is the primary interface for both human users and automation (CI/CD).
- **Automation First**: Designed with JSON-first output (`--json`) for machine consumption.
- **Deterministic**: Provides stable exit codes and predictable behavior.
- **Source of Truth**: All business operations are exposed through the CLI. Any other interface (like MCP) should ideally wrap the CLI or the Core library without duplicating logic.

### 3. ChangeSharp.MCP (AI Adapter Layer)
A lightweight adapter that implements the Model Context Protocol.
- **Interface Only**: It does not contain business logic.
- **AI-Ready**: Translates MCP tool calls into Core/CLI operations, making ChangeSharp accessible to AI agents (Cursor, Copilot, Claude).

---

## Technical Principles

### CLI as Source of Truth
We avoid putting logic in the MCP layer. If a feature is needed by an AI agent, it must first be available in the CLI or Core. This ensures that ChangeSharp remains fully functional in headless CI/CD environments where an AI agent might not be present.

### Robust Parsing
By using **Markdig** instead of regex-based parsing, we ensure that the Markdown structure of fragments and changelogs is respected, even with complex content (nested lists, blocks).
