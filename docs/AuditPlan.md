# ChangeSharp Audit & Correction Plan

> Generated from an exhaustive ("autistic") code review.
> Use this document to track fixes, prioritize work, and verify completeness before releases.

---

## Priority Legend

| Icon | Meaning |
|------|---------|
| 🔴 | **Critical** — data loss, silent wrong output, security bypass |
| 🟠 | **High** — incorrect behavior, broken edge case, doc/code mismatch |
| 🟡 | **Medium** — papercut, inconsistency, missing test, code smell |
| 🔵 | **Low** — cosmetic, naming, minor test hygiene |

---

## A. Bug Fixes (Correctness)

### A1. 🔴 `MSBuildVersionHandler` — `<Version>` vs `<VersionPrefix>` coexistence
**File:** `ChangeSharp/VersionPropagation/MSBuildVersionHandler.cs:28-29`

When a `.csproj` defines both `<Version>` and `<VersionPrefix>`, the handler updates `<Version>` but leaves `<VersionPrefix>` stale. In SDK-style projects, `VersionPrefix` takes precedence, so the version propagation is silently ignored at build time.

**Fix:** Update `VersionPrefix` elements too when both exist. Prefer `VersionPrefix` over `Version` when deciding which to add for SDK-style projects.

**Test:** Add `MSBuildHandler_UpdatesVersionPrefix_WhenBothExist` to `VersionPropagationTests.cs`.

---

### A2. 🔴 `Release()` — No validation before moving fragments
**File:** `ChangeSharp/WorkspaceManager.cs:425-428`

Fragments are moved from `unreleased/` to `releasing/` *before* any validation. A malformed fragment causes a partial release (files already moved, no rollback).

**Fix:** Call `Validate()` (or at minimum `ChangelogParser.Parse()`) on each fragment before the move. If any fragment fails, abort and do not move.

**Test:** Add `Release_WithInvalidFragment_ThrowsBeforeMovingFiles` to `WorkspaceManagerTests.cs`.

---

### A3. 🟠 `forcedVersion` ignored on resume path
**File:** `ChangeSharp/WorkspaceManager.cs:449-452`

When `Release()` detects the current version content matches the merged changeset (resume scenario), it sets `nextVersion = currentVersion` and ignores the `forcedVersion` parameter. The caller expects the forced version.

**Fix:** When `forcedVersion` is provided, use it regardless of the resume detection.

**Test:** Add `Release_ForcedVersion_RespectedOnResume` to `WorkspaceManagerTests.cs`.

---

### A4. 🟠 `ChangelogParser.Deindent` corrupts content lines starting with `#`
**File:** `ChangeSharp/ChangelogParser.cs:81,105`

The `Deindent` method treats *any* line starting with `#` (after trimming) as a heading. A content line like `  # Comment in text` gets trimmed to `# Comment in text`, then excluded from de-indentation. It becomes indistinguishable from a real `#` heading.

**Fix:** Track original line indices or use a regex-based heading detection. Only trim lines that match `^#{1,6}\s` at the original indent level.

**Test:** Add test in `ChangelogParser` test class (currently none exists) for content lines containing `#`.

---

### A5. 🟠 `NextVersionComputer` — Warning lost in `ComputeVersion` overload
**File:** `ChangeSharp/NextVersionComputer.cs:14-16`

`ComputeVersion` calls `ComputeVersionWithWarning` and discards the warning. Any `currentVersion` parsing failure silently falls back to `0.0.0`.

**Fix:** Make `ComputeVersionWithWarning` the canonical method, or throw on invalid input. Deprecate the warning-free overload or make it log.

**Test:** Update `GetNextSemanticVersionTests.cs` to test invalid current version input (should warn or throw).

---

### A6. 🟡 `Release()` leaves CHANGELOG.md without `[Unreleased]` section
After `ReleaseWithVersion`, no `## [Unreleased]` header is re-inserted into CHANGELOG.md. While fragments continue to work from `.changesharp/unreleased/`, the CHANGELOG.md cosmetic section is missing.

**Fix:** After inserting the new version section, re-append an empty `## [Unreleased]` section above it.

**Test:** Add `Release_ReaddsUnreleasedSection` to `ChangeLogTests.cs`.

---

### A7. 🟡 `CheckApiMinLevel` runs before dry-run in CLI
**File:** `ChangeSharp.Cli/Program.cs:300`

`--api-min-level` validation fires even in `--dry-run` mode, blocking the preview. A user expecting a no-side-effects preview gets an error.

**Fix:** Move the `apiMinLevel` check inside both the dry-run and non-dry-run branches, or skip it in dry-run mode.

**Test:** Add `Release_DryRun_SkipsApiMinLevel` to `DryRunTests.cs`.

---

## B. Security & Safety

### B1. 🟠 MCP server — releases blocked by default with confusing config
**File:** `ChangeSharp.Mcp/Program.cs:186-208` + `ChangeSharp/ChangeSharpConfig.cs:19`

`SecurityConfig.AllowAgentRelease` defaults to `false`. Combined with `config.Security.RequireApproval || config.Security.AllowAgentRelease == false`, the MCP server is effectively **read-only by default**. The user must set BOTH `AllowAgentRelease: true` in config AND `CHANGESHARP_ALLOW_UNSAFE_RELEASE=true` env var.

**Fix:** Either:
- a) Default `AllowAgentRelease` to `true` (the env var check is already a safety gate), or
- b) Document the double-gate prominently in `docs/McpIntegration.md`.

**Test:** Add test for MCP release gate logic (integration test or unit test on the decision logic).

---

### B2. 🟡 `JsonVersionHandler` creates JSON structure silently on missing path
**File:** `ChangeSharp/VersionPropagation/JsonVersionHandler.cs:48`

If `JsonPath` points to a non-existent path (e.g., typo in config), the handler creates the entire parent structure with empty objects instead of warning.

**Fix:** At minimum, warn when creating intermediate nodes. Consider failing for production use.

**Test:** Add `JsonHandler_WithMissingPath_Warns` to `VersionPropagationTests.cs`.

---

## C. Documentation / Code Mismatch

### C1. 🟠 `Changed` default: code says `Minor`, project config overrides to `Major`, docs say "Minor"
- Code (`ChangeSharpConfig.cs:34`): `"Changed" -> "Minor"`
- Project config (`changesharp.json:41`): `"Changed": "Major"`
- Docs (`docs/SemVer Rules.md:9`): correctly says Minor now, but `docs/features/CustomCategories.md:20,45` says Major.
- `docs/Roadmap.md:27`: mentions the fix but outdated references remain.

**Fix:** Audit ALL docs that mention the `Changed` default. Update `docs/features/CustomCategories.md` to match the code. Ensure single source of truth.

---

## D. Performance & Edge Cases

### D1. 🟡 `RegexVersionHandler` replaces ALL matches, not just the first
**File:** `ChangeSharp/VersionPropagation/RegexVersionHandler.cs:24`

`Regex.Replace` with no count parameter replaces every match in the file. If the pattern matches multiple times (e.g., version in comments, build configs), all are replaced, potentially corrupting unrelated content.

**Fix:** Document this behavior. Optionally add a `MatchIndex` property to `VersionTargetConfig` to select which match to replace.

**Test:** Add `RegexHandler_ReplacesOnlyFirstMatch` test.

---

### D2. 🟡 `GetGitBranchName()` — silent fail on non-git directory
**File:** `ChangeSharp/WorkspaceManager.cs:346-349`

The `catch {}` swallows all exceptions. If `git` is not installed or the directory is not a git repo, branch detection silently returns `null`. The fragment is still created but without branch info.

**Fix:** Log a warning to stderr (or return it through a warning out-parameter). The silent failure is acceptable but should be observable.

---

## E. Test Coverage Gaps

### E1. 🔴 No test for MSBuild handler when both `<Version>` and `<VersionPrefix>` exist
See A1. **Untested** bug.

### E2. 🟠 No test for `JsonVersionHandler` with hierarchical `JsonPath`
Current tests only test flat paths (`"version"`, `"appVersion"`). Nested paths like `"$.meta.version"` are untested.

### E3. 🟠 No test for `RegexVersionHandler` with `Regex == null`
The handler returns a warning string, but the code path is untested.

### E4. 🟠 No test for `ChangelogParser` with no headings (just text)
Behavior of `Parse("plain text")` is undefined in tests.

### E5. 🟠 No test for `CreateFragment` with message > 50 chars
The slug truncation at 50 characters (`Slugify`) is untested.

### E6. 🟡 No integration test through the CLI
All tests go through `WorkspaceManager` directly. No test runs `Program.Main(args)`.

### E7. 🟡 No test for `ChangelogParser.Deindent` behavior
The `Deindent` method (critical for parser correctness) has zero direct tests.

### E8. 🟡 `DryRunTests.cs` doesn't test `prerelease --dry-run`
Only `release --dry-run` is tested.

### E9. 🔵 Test method naming issues
| File | Line | Current Name | Suggested Name |
|---|---|---|---|
| `GetNextSemanticVersionTests.cs` | 11 | `ItComputeAMinorFromChanged` | `ItComputesAMinorFromChanged` |
| `GetNextSemanticVersionTests.cs` | 53 | `ItComputeAMajorrFromRemoved` | `ItComputesAMajorFromRemoved` |
| `GetNextSemanticVersionTests.cs` | 182,207 | `ItComputeAPath...` | `ItComputesAPatch...` |
| `ChangeSetMergerTests.cs` | 6 | `ItComputeAMajorFromChanged` | `Merge_CombinesChangesetsInOrder` |
| `ChangeLogTests.cs` | 93,171,244 | `### Secirity` | `### Security` |

---

## F. Code Smells & Maintainability

### F1. 🟡 `Console.Error.WriteLine` in domain layer
**File:** `ChangeSharp/WorkspaceManager.cs:609, 654`

`WorkspaceManager` (business logic) writes directly to `Console.Error` for corrupt prerelease info files. This couples the domain to the console.

**Fix:** Return warnings from `LoadConfig` / `ListPrereleases` methods, or pass an `ILogger`.

### F2. 🟡 `Output.Err` uses reflection for extra data serialization
**File:** `ChangeSharp.Cli/Program.cs:594`

`extra.GetType().GetProperties()` is fragile, slow, and not idiomatic. The extra data could be serialized directly via anonymous types passed to a `JsonSerializer`.

**Fix:** Accept `object?` and serialize with `System.Text.Json` directly, or use a dictionary-based approach.

### F3. 🟡 `changesharp.json` serializes null properties
`VersionTargetConfig` fields `Regex`, `JsonPath`, `Replacement` are `string?` and serialize as `"Regex": null, "JsonPath": null` for every target. Noisy.

**Fix:** Add `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` on nullable properties.

### F4. 🟡 `ChangeSet.Breaking`, `Changed`, etc. — hardcoded category properties
**File:** `ChangeSharp/ChangeSet.cs:9-15`

The strongly-typed properties like `Breaking`, `Changed`, `Added` assume standard English category names. Custom categories (supported by `SemverPolicyConfig.Mappings`) have no property accessor. Code paths using `changeset.Breaking` instead of `changeset.GetSection("Breaking Changes")` fail silently for custom categories.

**Fix:** Deprecate properties or make them delegates to `GetSection`.

---

## G. Feature Gaps

### G1. 🟡 CLI cannot specify release date
`Program.cs:340` uses `DateTime.Today` with no way to override. Backdating or CI-pinned dates impossible.

### G2. 🟡 MCP server does not expose `prerelease`, `remove`, or `init` tools
Only 4 tools exposed (`get_status`, `create_fragment`, `validate_fragments`, `perform_release`). Feature parity gap with the CLI.

### G3. 🟡 `--custom-category` flag missing from `new` command
Custom categories (e.g., `Maintenance`, `Documentation`) can only be used via MCP or direct API. CLI has no `--custom` flag.

---

## H. Verification Checklist

Use this checklist before each release:

- [ ] A1: MSBuild Dual Element fix applied + tested
- [ ] A2: Pre-move validation in Release + tested
- [ ] A3: forcedVersion resume fix + tested
- [ ] A4: Deindent heading detection fix + tested
- [ ] A5: ComputeVersion warning not lost + tested
- [ ] A7: dry-run + api-min-level decoupled + tested
- [ ] B1: MCP release security config reviewed + documented
- [ ] B2: JSON handler missing path warning + tested
- [ ] C1: All docs match code for `Changed` default
- [ ] D2: Git branch failure observable
- [ ] E1-E9: Test gaps filled
- [ ] F1-F4: Code smells addressed
- [ ] G1-G3: Feature gaps evaluated for scope

---

*Last updated: 2026-06-24*
*Audit method: manual code review of all 2,392 lines of production code and 1,650 lines of test code.*
