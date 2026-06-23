# Continuous Discovery and Smart Init

ChangeSharp is designed to evolve with your project. As you add new components (projects, packages) to your workspace, ChangeSharp can automatically discover and track them.

## Auto-discovery on Init

The `changesharp init` command is the entry point for configuring your workspace. It scans the current directory and its subdirectories for common project files:

- `.csproj` (C# Projects)
- `package.json` (Node.js/NPM packages)
- `Directory.Build.props` (MSBuild common properties)

These are automatically added to the `VersionTargets` in your `changesharp.json`.

## Additive Initialization

`init` is **additive**. If you run it in a workspace that already has a `changesharp.json`, it will:
1. Scan for components.
2. Filter out those already tracked in the configuration.
3. Add only the **newly discovered** components.
4. Leave existing configurations (like custom regex or versioning policies) untouched.

## Smart Warnings

The `changesharp status` command performs a background check for untracked components. If it finds a `.csproj` or `package.json` file that is not listed in your `changesharp.json`, it will display a warning:

```text
Warning: New components discovered but not tracked in changesharp.json:
  - NewService/NewService.csproj (MSBuild)
Run 'changesharp init' to add them to your configuration.
```

This ensures that you never forget to update your versioning configuration when the project structure grows.
