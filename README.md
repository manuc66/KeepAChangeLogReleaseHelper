# ChangeSharp

Derive the version. Keep a Changelog.

A .NET tool for changelog-driven semantic versioning.

## Quick start

```bash
dotnet tool install --global ChangeSharp.Cli
cd your-project
changesharp init
changesharp new --added "Initial setup"
changesharp release
```

## Documentation

See [docs/ChangeSharp.md](docs/ChangeSharp.md) for full documentation.

## Features

- **Changelog-driven versioning** — fragments in `.changesharp/unreleased/` drive SemVer bumps
- **CI/CD integration** — `--json` output, `--next-only`, `--require-fragments`
- **AI-ready** — built-in MCP server for AI agent integration
- **Version propagation** — MSBuild, JSON, and regex target handlers
- **Pre-release channels** — branch-based pre-release workflows
- **Safety gates** — `--api-min-level`, `--require-approval`, `--dry-run`

## License

MIT
