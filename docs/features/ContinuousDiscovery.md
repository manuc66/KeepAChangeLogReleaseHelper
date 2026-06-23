# Continuous Discovery and Smart Init

ChangeSharp is designed to evolve with your project. As you add new components (projects, packages) to your workspace, ChangeSharp can automatically discover and track them.

## Auto-discovery on Init

The `changesharp init` command is the entry point for configuring your workspace. It scans the current directory and its subdirectories for common project files:

- `.csproj` (C# Projects)
- `package.json` (Node.js/NPM packages)
- `Directory.Build.props` (MSBuild common properties)

These are automatically added to the `VersionTargets` in your `changesharp.json`.

## 🌍 Polyglot Support (Non-.NET Projects)

While ChangeSharp has native handlers for the files above, it is designed to be **polyglot-first**. You can track versions in any text file using the `Regex` handler in `changesharp.json`.

### Examples for Common Ecosystems

#### Python (`pyproject.toml`)
```json
{
  "Path": "pyproject.toml",
  "Handler": "Regex",
  "Regex": "version\\s*=\\s*\"(?<version>.*?)\""
}
```

#### Rust (`Cargo.toml`)
```json
{
  "Path": "Cargo.toml",
  "Handler": "Regex",
  "Regex": "^version\\s*=\\s*\"(?<version>.*?)\""
}
```

#### Java (`pom.xml`)
```json
{
  "Path": "pom.xml",
  "Handler": "Regex",
  "Regex": "<version>(?<version>.*?)</version>"
}
```

These examples ensure ChangeSharp is a first-class citizen in any multi-language monorepo or enterprise environment. Native handlers for these formats are planned in the roadmap.

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
