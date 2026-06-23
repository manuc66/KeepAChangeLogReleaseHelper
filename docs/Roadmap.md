# Roadmap to Production

To build a reliable MVP, we will prioritize robust parsing, extensible configuration, and developer experience.

### Step 1: Create the CLI (.NET Tool) & Error Contract (Completed)
* Initialize `ChangeSharp.Cli` as a console application configured as a packable tool (`<PackAsTool>true</PackAsTool>`).
* Set up command handling with `System.CommandLine`.
* **Define Exit Codes Contract**: Establish stable exit codes (e.g., `0` for success, `1` for generic error, `2` for no changes to release). This is critical for CI/CD integration.
* See [[ExitCodes|Exit Codes Specification]].

### Step 2: Integrate Markdig (Early Parsing Layer) (Completed)
* Replace the manual line-by-line parser with **Markdig**.
* This guarantees correct handling of rich Markdown elements (nested lists, code blocks, bold text) inside fragment sections right from the start, avoiding complex edge-case bug fixing later.

### Step 3: Fragment Lifecycle & UX (Workflow) (Completed)
* **Initialize** (`changesharp init`): Sets up the configuration file and the `.changesharp/unreleased/` directory.
* **Status** (`changesharp status`): Lists pending fragments, previews the calculated version bump, and shows config state. This feeds into `--dry-run` and MCP integration.
* **New Fragment** (`changesharp new`): 
    * Support interactive prompts for message and category (high-priority UX).
    * **Naming Strategy**: Implement a deterministic naming convention (e.g., `YYYYMMDD-slug.md`) to ensure sorting and avoid collisions. See [[FragmentNaming|Fragment Naming Strategy]].
* **Release** (`changesharp release`):
    * **Idempotence**: Implement a transaction-like strategy or state detection to handle crashes mid-release (e.g., after updating CHANGELOG but before archiving fragments).
    * Aggregate fragments, derive version, update targets, and archive consumed fragments.

### Step 4: Config, Propagation & SemVer Policy (Completed)
* Provide standard target handlers (MSBuild, JSON, text/regex) via `changesharp.json`.
* **SemVer Mapping**: Document and allow override of the `Changed` → `Major` policy. By default, "Changed" triggers a Major bump, but this must be configurable.
* **Principle**: Step 4 ensures the tool is extensible via configuration rather than hardcoded logic.

### Step 5: Dry-run Mode (Completed)
* Add `--dry-run` support to the `release` command.
* Allow users to preview the next version, aggregated changes, and affected version targets without making actual changes to the filesystem.

### Step 6: GitHub & GitLab CI/CD Integration (Completed)
* Created sample workflow files (`release.yml` for GitHub, `.gitlab-ci.yml` for GitLab) to automate the release process.
* **Focus**: Enable a standard "Release on Merge" or "Release on Tag" pipeline that is easy to adopt for DevOps engineers.
* Added `--next-only` and `--require-fragments` CLI options for better CI integration.

### Step 7: Pre-release Channels and Branch-based Versioning (Completed)
* See detailed specification: [[Features/Prereleases|Pre-release Feature]]

### Step 8: Smart Init (Auto-discovery) (Completed)
* Enhance the `init` command to scan the workspace for common project files (e.g., `.csproj`, `package.json`, `Directory.Build.props`).
* Automatically suggest and add these as `VersionTargets` in `changesharp.json`.
* **Continuous Discovery**: `init` is now additive and can be rerun at any time to discover and add new projects added to the repository after the initial setup. `status` also checks for untracked components and warns the user.
* See [[Features/ContinuousDiscovery|Continuous Discovery Specification]].
* **Frictionless Merge**: Strategy implemented to avoid conflicts on management files using isolated fragments and deferred versioning. See [[FrictionlessWorkflow|Frictionless Merge Workflow]].

### Step 9: Fragment Validation and Linting (Completed)
* Add a `lint` or `validate` command to check if unreleased fragments follow the expected Markdown structure.
* This must precede custom category support to ensure the validation engine is extensible.

### Step 10: Custom Categories and Mappings (Completed)
* Allow users to define custom changelog categories in `changesharp.json`.
* Enable mapping these custom categories to specific SemVer impacts (Major, Minor, or Patch).
* See [[Features/CustomCategories|Custom Categories Specification]].

### Step 11: AI Automation Layer (CLI + MCP) (Completed)

#### Goal
Make ChangeSharp fully automation-ready for CI/CD systems and AI agents without adding complexity to the core system.
* Implemented a lightweight MCP server in `ChangeSharp.Mcp`.
* Exposes tools for status, fragment creation, validation, and release.
* See [[McpIntegration|MCP Integration Documentation]].

---

#### Approach
ChangeSharp uses a **two-layer automation model**:

**1. CLI (primary interface)**
* JSON-first output (`--json`)
* deterministic, machine-readable results
* used by CI/CD and scripts
* contains all business logic via the core library

**2. MCP (AI adapter layer)**
* lightweight STDIO-based MCP server
* no business logic duplication
* acts as a thin wrapper over the CLI or core library
* exposes ChangeSharp features to AI agents (Cursor, Copilot, Claude, etc.)

---

#### Architecture
```
ChangeSharp.Core   → business logic
        ↑
ChangeSharp.CLI    → JSON-first automation interface (Source of Truth)
        ↑
ChangeSharp.MCP    → AI/tooling adapter (Interface only)
```

---

#### Key Principle
> **CLI is the source of truth for automation.**
> **MCP is only an interface layer for AI agents.**

---

### Step 12: Self-hosting & Dogfooding (Completed)
* Implement `CHANGELOG.md` for ChangeSharp using ChangeSharp itself.
* Successfully performed the first release (v1.0.0) using the tool.
* Symbolically important for a tool promoting this practice.

### Step 13: ChangeSharp Bot for Pull Requests (Priority 2)
* **Goal**: Ensure fragment quality and provide visibility into release impact during the PR phase.
* **Phase 1: CI-Native Bot (GitHub Actions/GitLab CI)**:
    * Implement a lightweight script/action that runs `changesharp validate`.
    * Post PR comments using standard CI tokens.
    * No external infrastructure required.
* **Phase 2: Full ChangeSharp App (Marketplace)**:
    * Build a dedicated GitHub App / GitLab Webhook service.
    * Support OAuth, webhook signature validation, and multi-repo management.
    * Provide a seamless "Install" experience from the marketplace.
* See [[Features/CiIntegration|CI/CD Integration Contract]].

### Step 14: Polyglot Support & First-Class Handlers (Priority 3)
* **Goal**: Move beyond "survivability" for non-.NET projects.
* **Actions**:
    * Implement native handlers for `pyproject.toml` (Python), `Cargo.toml` (Rust), and `pom.xml` (Java).
    * Formalize the Regex handler documentation with clear, copy-pasteable examples for any language.
    * Explore a simple plugin system for custom version targets.

### Step 15: Enterprise Security & Approval Gates (Completed)
* **Dry-run enforcement**: Implement a mandatory `--dry-run` or `--require-approval` flag for MCP `perform_release` tool.
* Ensure AI agents cannot trigger a production release without an explicit human gate in the loop.
* See [[Features/ApprovalGates|Enterprise Security & Approval Gates]].

### Step 16: Repository Intelligence for Agents (Priority 2)
* **Goal**: Allow AI agents to understand what has changed in the codebase since the last release, even if fragments are missing.
* **Features**:
    * `query_changes`: A tool to compare the current state with the last tag/release and identify modified files/functions.
    * **Fragment Gap Analysis**: Automatically suggest missing fragments based on uncommitted or unreleased code changes.
    * Summarize "What's New" from both a technical (Git) and user (Changelog) perspective.

### Step 17: Semantic API Validation (Safety Gate) (Priority 1)
* **Goal**: Ensure the promised SemVer impact in fragments matches the reality of the code changes.
* **Actions**:
    * Integrate with API surface tracking tools (e.g., `PublicApiGenerator` for .NET, Swagger diff for APIs).
    * **Cross-verification**: Fail the `validate` or `release` command if a fragment says `### Fixed` (Patch) but a public method signature was deleted (Major).
    * Provide "Fix-it" suggestions: "You've made a breaking change, please move this fragment to 'Breaking Changes' or 'Removed'."
* See [[Features/SemanticValidation|Semantic Validation Safety Gate]].

### Step 18: Multi-Team Monorepo Scoping (Priority 2)
* **Goal**: Support large-scale enterprise monorepos with independent team policies.
* **Actions**:
    * **Config Hierarchies**: Allow `changesharp.json` to be placed in subdirectories, overriding global settings for that scope.
    * **Scoped Fragments**: Support `.changesharp/` directories per service/sub-project.
    * **Service-Specific Changelogs**: Generate independent `CHANGELOG.md` files for different parts of the repository.
    * **Policy Overrides**: Team A might want `Changed -> Major` while Team B wants `Changed -> Minor`.
* See [[features/MonorepoSupport|Multi-Team Monorepo Support]].
