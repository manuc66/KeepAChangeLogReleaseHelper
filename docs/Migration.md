# Migration Guide: Moving to ChangeSharp

This guide helps you transition from commit-driven versioning tools (**GitVersion**, **MinVer**, **Semantic Release**) to the fragment-based approach of **ChangeSharp**.

## 1. The Mindset Shift

| From (Commit-Driven) | To (Fragment-Driven) |
|---|---|
| Version depends on Git history/tags | Version depends on documented changes |
| Release notes are generated from commits | Release notes are authored by developers |
| Merge conflicts on `CHANGELOG.md` | Conflict-free Markdown fragments |
| "Breaking Change" is a commit footer | "Breaking Change" is a fragment category |

## 2. Step-by-Step Migration

### Step A: Initialize ChangeSharp
Install the tool and create the base configuration.
```bash
dotnet tool install --global ChangeSharp.Cli
changesharp init
```

### Step B: Establish the "Current Version"
ChangeSharp needs to know where you are starting from.
1. Ensure your `CHANGELOG.md` has the latest version as the top header (e.g., `## [1.2.0] - 2024-06-20`).
2. If you don't use a changelog yet, create one with your current version.

### Step C: Stop using Conventional Commits (Optional)
You can still use them for developer history, but you no longer need them for versioning. You can now write more descriptive, human-readable commit messages without worrying about `feat:`, `fix:`, or `!` syntax.

### Step D: Update CI/CD
Replace your GitVersion/MinVer steps with ChangeSharp:
1. **Validation**: Run `changesharp validate` on Pull Requests.
2. **Release**: Run `changesharp release` on your main branch.

## 3. Comparison with Other Tools

### vs. GitVersion / MinVer
*   **Pros**: No more complex "tag-based" or "distance-from-main" calculations that can break if Git history is rewritten. Perfect for monorepos where multiple services have different release cycles.
*   **Cons**: Requires one extra file (the fragment) per change.

### vs. Changie / Towncrier
*   **Pros**: Deeply integrated into the .NET ecosystem (NuGet, MSBuild). Includes **Safety Gates** to verify API compatibility. AI-ready via MCP.

## 4. Adoption Strategy
We recommend a **Dual-Run** approach for one sprint:
1. Keep your existing versioning tool.
2. Start adding ChangeSharp fragments for every PR.
3. Verify that `changesharp status` predicts the same version your existing tool would.
4. Once the team is comfortable, remove the old tool.
