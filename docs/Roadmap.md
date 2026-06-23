# Roadmap to Production

To build a reliable MVP, we will prioritize robust parsing and extensible configuration:

### Step 1: Create the CLI (.NET Tool)
* Initialize `ChangeSharp.Cli` as a console application configured as a packable tool (`<PackAsTool>true</PackAsTool>`).
* Set up command handling with `System.CommandLine` (e.g., `init`, `new`, `status`, `release`).

### Step 2: Integrate Markdig (Early Parsing Layer)
* Replace the manual line-by-line parser with **Markdig**.
* This guarantees correct handling of rich Markdown elements (nested lists, code blocks, bold text) inside fragment sections right from the start, avoiding complex edge-case bug fixing later.

### Step 3: Fragment Lifecycle Management (Workflow)
* **Initialize** (`changesharp init`): Sets up the configuration file and the `.changesharp/unreleased/` directory.
* **New Fragment** (`changesharp new "add-feature"`): Generates a timestamped markdown template.
* **Release** (`changesharp release`):
  1. Aggregates all fragments from the unreleased directory.
  2. Derives the new version.
  3. Updates `CHANGELOG.md`.
  4. Archives/cleans up the consumed fragments.

### Step 4: Extensible Version Propagation (Completed)
* Avoid complex, hardcoded C# logic to update project files directly.
* Introduce an extensible configuration schema (e.g., in `changesharp.json`).
* Provide standard target handlers (MSBuild, JSON, text/regex) to safely propagate the derived version.

### Step 5: Dry-run Mode (Completed)
* Add `--dry-run` support to the `release` command.
* Allow users to preview the next version, aggregated changes, and affected version targets without making actual changes to the filesystem.

### Step 6: GitHub Actions Integration
* Create sample workflow files to automate the release process in CI/CD.

### Step 7: Pre-release Channels and Branch-based Versioning (Completed)
* See detailed specification: [[Features/Prereleases|Pre-release Feature]]

### Step 8: Interactive 'new' Command
* Enhance the `new` command to prompt for a message and category if they are not provided via arguments.
* Improve developer experience by making fragment creation as frictionless as possible.

### Step 9: Smart Init (Auto-discovery)
* Enhance the `init` command to scan the workspace for common project files (e.g., `.csproj`, `package.json`, `Directory.Build.props`).
* Automatically suggest and add these as `VersionTargets` in `changesharp.json`.

### Step 10: Fragment Validation and Linting
* Add a `lint` or `validate` command to check if unreleased fragments follow the expected Markdown structure.
* Ensure all fragments use recognized categories to avoid SemVer calculation errors.

### Step 11: Custom Categories and Mappings
* Allow users to define custom changelog categories in `changesharp.json`.
* Enable mapping these custom categories to specific SemVer impacts (Major, Minor, or Patch).
