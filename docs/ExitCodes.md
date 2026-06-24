# Exit Codes Specification

To ensure reliable integration with CI/CD pipelines, `ChangeSharp.Cli` uses stable and documented exit codes.

## Standard Exit Codes

| Code | Name | Description |
| :--- | :--- | :--- |
| `0` | **Success** | The command completed successfully. |
| `1` | **Generic Error** | An unexpected error occurred (crash, file access denied, etc.). |
| `2` | **No Changes** | (Command: `release`) No unreleased fragments found and `--allow-empty` was not specified. |
| `3` | **Validation Error** | Fragments or configuration failed validation. |
| `4` | **Conflict** | A version conflict or state inconsistency was detected. |

## Command-Specific Behavior

### `release` command
* By default, if no fragments exist in the unreleased directory, the command exits with code `2`.
* If `--allow-empty` is used, it will exit with code `0` even if no fragments are found (silently doing nothing).
* This allows CI/CD workflows to decide whether a "empty" release should fail the build or be ignored.

## Transactional Integrity & Resumability
If the `release` command is interrupted after moving fragments to the `releasing/` directory, the next call resumes from that state:

1. Fragments in `releasing/` are used instead of `unreleased/`.
2. If `CHANGELOG.md` already contains the computed version with matching content, the changelog update is skipped.
3. Version propagation runs **before** fragment cleanup, so a failure during propagation leaves fragments intact for debugging.

The CLI aims to be safe: a failed release will not silently lose fragments.
