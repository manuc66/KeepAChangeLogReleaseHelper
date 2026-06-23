# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Documented architecture with a three-layer model (Core, CLI, MCP).
- Established CLI as the source of truth for automation.
- Documented and implemented exit codes contract for CI/CD.
- Defined and implemented deterministic fragment naming strategy.
- Updated SemVer rules: `Changed` now defaults to `Major` (configurable via `SemverPolicy`).
- Implemented robust, transactional release process using a temporary `releasing` directory to handle interruptions.
- Added `--allow-empty` to `release` command.
- Self-hosting: Added this `CHANGELOG.md` to the project.
- Smart Init (Auto-discovery): `init` command now automatically detects `.csproj`, `package.json`, and `Directory.Build.props`.
- Continuous Discovery: `init` is now additive, and `status` warns about untracked components.
- Fragment Validation: New `validate` command to lint unreleased fragments.
- Custom Categories: Support for user-defined categories and custom SemVer impact mappings in `changesharp.json`.

### Changed
- Refined Roadmap structure (UX improvements and better ordering).
- Updated internal parsing logic to use Markdig.
