# ChangeSharp

> **Git commits are for developers. Changelogs are for users. ChangeSharp keeps them separate.**

**ChangeSharp** is a command-line tool (.NET Tool) and a C# library designed to aggregate independent changelog fragments (changesets) formatted according to the *Keep a Changelog* specification. It consolidates them into a global changelog file and automatically calculates the next release version.

---

## 📖 Documentation Index

### 💡 Core Concept
* [[ChangeSharp#Strategic Positioning|Strategic Positioning]]
* [[SemVer Rules|SemVer Derivation Rules]] - How fragments impact versioning.

### 🛠️ Development & Roadmap
* [[Architecture|Codebase Analysis]] - Technical overview of the project.
* [[Roadmap]] - Completed and planned steps.

### ✨ Key Features
* [[Features/Prereleases|Pre-release & Branching]] - Detailed spec for SemVer pre-releases.

---

## 🚀 Strategic Positioning: Why Choose ChangeSharp?

In the .NET ecosystem, release versioning is heavily dominated by tools like **GitVersion** and **MinVer**. While powerful, they bind versioning and release notes directly to Git commit messages (often via *Conventional Commits*).

ChangeSharp offers a different philosophy:
* **Separation of Concerns**: Git history is for developer implementation details; changelogs are for user-facing value. They shouldn't be coupled.
* **No Merge Conflicts**: Branches add independent fragments as separate Markdown files. Merge conflicts on `CHANGELOG.md` are completely eliminated.
* **Dotnet Native**: Unlike `@changesets/cli` (JS) or `changie` (Go), ChangeSharp is built specifically for .NET projects.

#### Why This Matters
Unlike commit-driven versioning tools, ChangeSharp derives both releases and pre-releases from user-facing changelog fragments. This ensures that version numbers remain predictable, meaningful, and directly tied to documented changes rather than implementation details hidden in Git history.
