# Syntactic Validation Safety Gate

ChangeSharp ensures that your release notes and your code remain in sync. The **Syntactic Validation Safety Gate** is a feature designed to prevent "version drift" by cross-verifying the SemVer impact declared in fragments against the actual changes in the codebase.

> [!IMPORTANT]
> **Syntactic vs. Semantic**: This gate focuses on **syntactic** compatibility (API signatures, missing methods, type changes). It cannot detect **behavioral (semantic)** changes. For example, replacing `List.fold` with `List.foldBack` while maintaining the same signature will pass this gate but may break consumers. This tool provides a safety net for public API surfaces, not a guarantee of behavioral identity.

## The Problem

In a manual or agent-assisted workflow, it is easy to:
1.  Add a breaking change but forget to mark it as `### Breaking Changes` or `### Removed`.
2.  Add a new feature but mark it as `### Fixed` (Patch) instead of `### Added` (Minor).

These errors lead to incorrect SemVer numbers and broken downstream dependencies.

## The Solution

ChangeSharp integrates with external static analysis and diffing tools to audit the API surface.

### 1. Analysis Layer
ChangeSharp doesn't reinvent API diffing. Instead, it acts as an orchestrator for:
-   **.NET**: `PublicApiGenerator` or `Microsoft.DotNet.ApiCompat`.
-   **REST APIs**: `openapi-diff` or `swagger-diff`.
-   **CLI**: Schema comparison of help outputs.

### 2. Decision Engine
The tool compares the **Declared Impact** (from fragments) with the **Observed Impact** (from the analyzer).

| Declared (Fragment) | Observed (Code) | Result | Action |
| :--- | :--- | :--- | :--- |
| Patch | Patch | ✅ Pass | Proceed with release. |
| Minor | Patch | ⚠️ Warn | "You are bumping Minor but only Patch changes detected." |
| Patch | Minor | ❌ Fail | "New features detected. Upgrade fragment to 'Added'." |
| Minor | Major | ❌ Fail | "Breaking changes detected. Upgrade fragment to 'Removed'." |

## Configuration

In `changesharp.json`, you can define your validation command:

```json
{
  "Validation": {
    "SyntacticGate": {
      "Enabled": true,
      "Tool": "dotnet-api-diff",
      "Arguments": "--base-tag last-release --current-dir .",
      "Mode": "Enforce"
    }
  }
}
```

## Reliability & Environment Control

In enterprise CI runners, external tools might not always be available or properly configured. ChangeSharp follows a strict reliability contract for the Syntactic Gate:

### ⚙️ Behavior on Tool Failure

You can configure how ChangeSharp reacts if an external analyzer fails or is missing via the `Mode` setting:

-   **`Enforce` (Default)**: If the tool is missing or returns a non-zero exit code, ChangeSharp will exit with **Exit Code 1**. This prevents accidental releases when the safety gate is broken.
-   **`Warn`**: ChangeSharp will display a warning but proceed with the validation/release. Useful during migrations or if the tool is flaky.
-   **`Disabled`**: The gate is completely skipped.

### 🔍 Tool Availability Check

ChangeSharp performs a "Pre-flight Check" before running the gate. If `Tool` is not found in the `PATH`, it will fail immediately with a descriptive error:
`Error: Syntactic Validation tool 'openapi-diff' not found in PATH. Ensure it is installed in your CI runner.`

## Continuous Integration

This gate is intended to be run:
1.  **Locally**: During `changesharp validate` or `changesharp release --dry-run`.
2.  **In PRs**: By the **ChangeSharp Bot** to notify developers of mismatching fragments before merge.
3.  **In AI Workflows**: By the MCP server to ensure agents don't accidentally release breaking changes without proper documentation.
