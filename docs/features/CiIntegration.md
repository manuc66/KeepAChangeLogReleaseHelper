# CI/CD Integration Contract (Step 6)

This document defines how ChangeSharp integrates into enterprise CI/CD pipelines (GitHub Actions, GitLab CI).

## 🚀 The Standard Workflow

ChangeSharp is designed to facilitate a "Release on Merge" or "Release on Tag" workflow.

### 1. Pull Request / Merge Request Phase
**Goal**: Ensure every change is documented and valid before it reaches the main branch.

-   **Command**: `changesharp validate`
-   **Contract**:
    -   **Exit Code 0**: All fragments are valid and present.
    -   **Exit Code 3**: Missing fragment (PR contains code changes but no `.changesharp/unreleased/*.md` file).
    -   **Exit Code 4**: Invalid fragment format (e.g., wrong headers, invalid SemVer impact).
-   **CI Action**: Post a ❌ on the PR if exit code is non-zero. Use the predicted version bump in the comment.

### 2. Post-Merge / Release Phase
**Goal**: Finalize the release, update the changelog, and bump versions.

-   **Workflow**:
    1.  **Dry-run Validation**: `changesharp release --dry-run`
        - Ensures the environment is ready (secrets, git permissions).
    2.  **Approval Gate**: (Manual step in GitHub/GitLab UI)
    3.  **Perform Release**: `changesharp release`
        - Aggregates fragments.
        - Updates `CHANGELOG.md` and project files (`.csproj`, `package.json`, etc.).
        - Moves fragments to `.changesharp/released/`.
    4.  **Git Tag & Push**: (Scripted)
        - `git add . && git commit -m "chore: release v1.2.0"`
        - `git tag v1.2.0 && git push --follow-tags`

---

## 🔐 Environment & Secrets

To function correctly in CI, ChangeSharp requires:

| Variable | Requirement | Description |
| :--- | :--- | :--- |
| `GITHUB_TOKEN` | Required | For the bot to post comments on PRs. |
| `GIT_AUTHOR_NAME` | Required | For the release commit. |
| `GIT_AUTHOR_EMAIL` | Required | For the release commit. |
| `CHANGESHARP_CONFIG` | Optional | Override `changesharp.json` path. |

---

## 🛠️ Sample GitHub Action (`release.yml`)

```yaml
name: Release
on:
  push:
    branches: [main]

jobs:
  release:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      
      - name: Validate Release
        run: dotnet changesharp release --dry-run
        
      - name: Perform Release
        run: |
          dotnet changesharp release
          git config user.name "github-actions"
          git config user.email "github-actions@github.com"
          git add .
          git commit -m "chore: release $(dotnet changesharp status --next-only)"
          git push
          git tag v$(dotnet changesharp status --next-only)
          git push --tags
```

---

## ⚠️ Reliability & Error Handling

-   **Missing External Tools**: If a configured tool (e.g., for Semantic Validation) is missing, ChangeSharp will **Exit Code 1** by default. This can be configured to **Warn** in `changesharp.json` to avoid breaking pipelines during environment migration.
-   **Idempotence**: Running `release` twice on the same state will result in **Exit Code 2** (No changes to release), which is safe and should not fail the pipeline.
