# SemVer Derivation Rules

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
