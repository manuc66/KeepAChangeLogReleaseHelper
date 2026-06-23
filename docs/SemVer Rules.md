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

You can override these default mappings in your `changesharp.json` configuration file to better suit your project's versioning policy. For example, if you follow a more relaxed SemVer approach where "Changed" is always backward-compatible, you can map it to `Minor`.
