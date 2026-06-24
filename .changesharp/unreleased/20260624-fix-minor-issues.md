### Changed
- Remove unused System.CommandLine.NamingConventionBinder dependency
- Add PackAsTool to ChangeSharp.Mcp
- Remove unnecessary Version from test project
### Fixed
- MCP: consistent DateTime.Today, ToChangelogString, dynamic server version
- ChangeLog.UpdateUnReleased: handle both \r\n and \n newlines
- CreatePrerelease: warn on corrupt info.json instead of silent reset
- SanitizeBranchName: avoid redundant LoadConfig call
### Added
- Repository root README.md
