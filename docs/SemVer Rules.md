# SemVer Derivation Rules

To avoid the pitfalls of using *Keep a Changelog* categories directly for SemVer (where a simple change would trigger a false-positive Major bump), ChangeSharp uses a refined, pragmatic mapping:

| Fragment Section | SemVer Impact | Description / Discipline |
| :--- | :--- | :--- |
| `### Breaking Changes` | ⬆️ **Major** | Explicit breaking changes. Kept as a separate section in the fragment and compiled into the final release notes for clear visibility. |
| `### Removed` | ⬆️ **Major** | Removing a documented public feature is a breaking change. |
| `### Changed` | ⬆️ **Major** | Modifications to existing features. **Note**: By default, ChangeSharp treats "Changed" as a Major bump to be conservative. This can be overridden to "Minor" in `changesharp.json`. |
| `### Added` | ➡️ **Minor** | New backward-compatible features. |
| `### Deprecated` | ➡️ **Minor** | Warnings about future removals. |
| `### Fixed` | 🛞 **Patch** | Bug fixes. |
| `### Security` | 🛞 **Patch** | Security improvements. |

## Customization

You can override these default mappings or define entirely new categories in your `changesharp.json` configuration file. This is useful for internal maintenance, documentation changes, or specific project workflows.

```json
{
  "SemverPolicy": {
    "Mappings": {
      "Breaking Changes": "Major",
      "Added": "Minor",
      "Maintenance": "Patch",
      "Documentation": "None"
    }
  }
}
```

Impact levels supported: `Major`, `Minor`, `Patch`, `None`.

---

## 🛡️ Automated Verification (Safety Gates)

Human error is the main cause of SemVer violations (e.g., forgetting that a change is breaking). ChangeSharp aims to provide an automated **Safety Gate** to cross-verify fragments against actual code changes.

### How it works
During the `validate` or `release` process, ChangeSharp can be configured to run external tools that analyze the API surface:

1.  **Extract Expected Impact**: ChangeSharp reads the pending fragments (e.g., `### Added` implies Minor).
2.  **Analyze Real Impact**: An external analyzer (like `PublicApiGenerator` or a Swagger diff tool) compares the current code against the last released version.
3.  **Cross-Check**:
    *   If Real Impact > Expected Impact (e.g., Code says Major, Fragment says Patch) -> **FAIL** with a clear error.
    *   If Real Impact < Expected Impact -> **WARN** (the user might be over-bumping intentionally).

### Integration Examples
*   **Web APIs**: Compare Swagger/OpenAPI schemas.
*   **.NET Libraries**: Use `PublicApiGenerator` to detect signature changes.
*   **CLI Tools**: Compare help output or command schemas.
