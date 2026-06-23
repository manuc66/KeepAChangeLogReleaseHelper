# ChangeSharp

> **Git commits are for developers. Changelogs are for users. ChangeSharp keeps them separate.**

**ChangeSharp** is a command-line tool (.NET Tool) and a C# library designed to aggregate independent changelog fragments (changesets) formatted according to the *Keep a Changelog* specification. It consolidates them into a global changelog file and automatically calculates the next release version.

---

## 📖 Documentation Index

### 💡 Core Concept
* [[ChangeSharp#Strategic Positioning|Strategic Positioning]]
* [[SemVer Rules|SemVer Derivation Rules]] - How fragments impact versioning.
* [[features/SemanticValidation|Semantic Safety Gates]] - Cross-verifying code vs fragments.

### 🛠️ Development & Roadmap
* [[Architecture|Codebase Analysis]] - Technical overview of the project.
* [[Roadmap]] - Completed and planned steps.

### ✨ Key Features
* [[Features/Prereleases|Pre-release & Branching]] - Detailed spec for SemVer pre-releases.

---

## 🚀 Strategic Positioning: Why Choose ChangeSharp?

In the .NET ecosystem, release versioning is heavily dominated by tools like **GitVersion** and **MinVer**. While powerful, they bind versioning and release notes directly to Git commit messages.

ChangeSharp offers a different philosophy focused on **Developer Experience** and **Enterprise Workflow**:

*   **Separation of Concerns**: Git history is for developer implementation details; changelogs are for user-facing value.
*   **Conflict-Free Workflows**: Branches add independent fragments as separate Markdown files, eliminating merge conflicts on `CHANGELOG.md`.
*   **CI/CD First (Native Bot Story)**: Designed to integrate into PR/MR workflows with native bots that validate fragments and preview release impact before merging.
*   **Enterprise-Ready Security**: Built-in support for approval gates and dry-runs, especially when using AI agents via MCP.
*   **Polyglot Ambition**: While starting as .NET native, ChangeSharp aims to be the universal changelog manager with first-class support for Python, Rust, and Java via specialized handlers.

#### Why This Matters
Unlike commit-driven versioning tools, ChangeSharp derives both releases and pre-releases from user-facing changelog fragments. This ensures that version numbers remain predictable, meaningful, and directly tied to documented changes rather than implementation details hidden in Git history.
