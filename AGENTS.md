# Agent Guardrails

High-level rules to prevent the systemic issues found in [CodeReview.md](docs/CodeReview.md).

## 1. Documentation & code must match

Before marking a feature "Completed" in any doc or roadmap, verify the implementation exists in the code. If you document a CLI flag (`--json`, `--require-approval`, etc.), implement it — or don't document it.

- **Doc-first features** must include a test that exercises the documented behavior.
- **Code-first changes** must update any doc that references the changed API, default, or workflow.
- When adding a default value to `ChangeSharpConfig.cs`, check that `docs/SemVer Rules.md` and `changesharp.json` match.

## 2. Never swallow exceptions silently

- `catch { }` or `catch { return default; }` hides bugs. Always log or rethrow.
- `LoadConfig` must not return a silent default on corrupt JSON. Report the error.
- Version parsing failures must not silently produce `0.0.0`. Warn or fail.

## 3. Single source of truth for defaults

Every default value (e.g. `Changed → Minor`) must be defined in **exactly one place**: the code (`ChangeSharpConfig.cs`). Docs reference that default; they do not duplicate or contradict it.

- Before changing a default in code, update the doc in the same commit.
- Before changing a default in a doc, verify the code agrees.

## 4. Fragments before commits

Every logical change must have its own fragment **before** the commit. Group related fragments into a single commit only when they implement the same feature.

- Use `changesharp new --added` / `--changed` / `--fixed` per change.
- Commit message format: `type(scope): description` (e.g. `feat(cli): add --json output`).

## 5. CI samples must match the project target

When the project's `TargetFramework` changes, update every CI sample (`samples/ci/*`) that references the old framework version — in the same commit.

## 6. Every new public API needs a test

- `WorkspaceManager` methods → `WorkspaceManagerTests.cs`
- Version handlers → `VersionPropagationTests.cs`
- Commands go through the CLI layer; test the underlying `WorkspaceManager` method.

## 7. No dead promises in the roadmap

`docs/Roadmap.md` must not mark steps "Completed" unless all documented sub-items exist in the code and are tested. When de-scoping a feature, update the roadmap to reflect the actual scope.
