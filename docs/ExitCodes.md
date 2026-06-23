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

## Transactional Integrity & Idempotence
If a command (like `release`) is interrupted, the next call will attempt to detect the intermediate state:
1. Check if `CHANGELOG.md` was updated but fragments were not archived.
2. Check if version propagation happened but `CHANGELOG.md` was not updated.

The CLI aims to be idempotent: running `release` twice on the same state should not create two releases.
