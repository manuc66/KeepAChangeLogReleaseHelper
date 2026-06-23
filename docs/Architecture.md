# Codebase Analysis

The core logic is clean, focused, and fully tested:
- `ChangeLog.cs`: Represents the global changelog file and handles insertion operations (updating *Unreleased* and generating a release).
- `ChangeSet.cs`: Models the changes categorized by section.
- `ChangelogParser.cs`: Reconstructs a `ChangeSet` from Markdown input.
- `ChangeSetMerger.cs`: Merges multiple `ChangeSet` instances.
- `NextVersionComputer.cs`: Computes the next version based on populated sections.
- `WorkspaceManager.cs`: Orchestrates the filesystem operations, configuration, and the overall release workflow.
- `VersionPropagation/`: Contains handlers for updating version numbers in various file types (MSBuild, JSON, Regex).
