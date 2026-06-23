# ChangeSharp

> **Git commits are for developers. Changelogs are for users. ChangeSharp keeps them separate.**

**ChangeSharp** is a command-line tool (.NET Tool) and a C# library designed to aggregate independent changelog fragments (changesets) formatted according to the *Keep a Changelog* specification. It consolidates them into a global changelog file and automatically calculates the next release version.

---

## 💡 Concept and Originality

### 1. Strategic Positioning: Why Choose ChangeSharp?
In the .NET ecosystem, release versioning is heavily dominated by tools like **GitVersion** and **MinVer**. While powerful, they bind versioning and release notes directly to Git commit messages (often via *Conventional Commits*).

ChangeSharp offers a different philosophy:
* **Separation of Concerns**: Git history is for developer implementation details; changelogs are for user-facing value. They shouldn't be coupled.
* **No Merge Conflicts**: Branches add independent fragments as separate Markdown files. Merge conflicts on `CHANGELOG.md` are completely eliminated.
* **Dotnet Native**: Unlike `@changesets/cli` (JS) or `changie` (Go), ChangeSharp is built specifically for .NET projects. It can be installed as a `dotnet tool`, integrates natively with MSBuild, and runs without installing Node.js, Python, or Go runtimes in your CI/CD pipelines.

---

## ⚙️ SemVer Derivation Rules

To avoid the pitfalls of using *Keep a Changelog* categories directly for SemVer (where a simple change would trigger a false-positive Major bump), ChangeSharp uses a refined, pragmatic mapping:

| Fragment Section | SemVer Impact | Description / Discipline |
| :--- | :--- | :--- |
| `### Breaking Changes` | ⬆️ **Major** | Explicit breaking changes. Kept as a separate section in the fragment and compiled into the final release notes for clear visibility. |
| `### Removed` | ⬆️ **Major** | Removing a documented public feature is a breaking change. *Note: Internal removals should be kept out of the public changelog.* |
| `### Changed` | ➡️ **Minor** | Modifications to existing features that do not break backward compatibility. |
| `### Added` | ➡️ **Minor** | New backward-compatible features. |
| `### Deprecated` | ➡️ **Minor** | Warnings about future removals. |
| `### Fixed` | 🛞 **Patch** | Bug fixes. |
| `### Security` | 🛞 **Patch** | Security improvements. |

This ensures that developers get zero-configuration version derivation while maintaining high trust in version numbers.

---

## 🛠️ Codebase Analysis

The core logic is clean, focused, and fully tested:
- [ChangeLog.cs](file:///home/manu/sources/KeepAChangeLogReleaseHelper/ChangeSharp/ChangeLog.cs): Represents the global changelog file and handles insertion operations (updating *Unreleased* and generating a release).
- [ChangeSet.cs](file:///home/manu/sources/KeepAChangeLogReleaseHelper/ChangeSharp/ChangeSet.cs): Models the changes categorized by section.
- [ChangelogParser.cs](file:///home/manu/sources/KeepAChangeLogReleaseHelper/ChangeSharp/ChangelogParser.cs): Reconstructs a `ChangeSet` from Markdown input.
- [ChangeSetMerger.cs](file:///home/manu/sources/KeepAChangeLogReleaseHelper/ChangeSharp/ChangeSetMerger.cs): Merges multiple `ChangeSet` instances.
- [NextVersionComputer.cs](file:///home/manu/sources/KeepAChangeLogReleaseHelper/ChangeSharp/NextVersionComputer.cs): Computes the next version based on populated sections.

---

## 🚀 Roadmap to Production

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

### Step 7: Pre-release and Version Prefix Support
* Handle SemVer pre-release tags (e.g., `-beta.1`).
* Support for `v` prefix in version numbers (e.g., `v1.2.3`).
