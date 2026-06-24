# Fragment Naming Strategy

The convention for naming unreleased fragment files is designed for readability, easy sorting, and collision avoidance.

## Standard Format

Fragments follow this pattern:
`YYYYMMDDHHmmssfff-[slug].md`

Example: `20260624143005123-add-mcp-server.md`

### Why this format?
1. **Chronological Sorting**: Timestamps ensure that files are listed in the order they were created when looking at the directory.
2. **Collision Avoidance**: Millisecond precision (`fff`) makes collisions extremely unlikely, even in multi-developer environments.
3. **Contextual Readability**: The `slug` (derived from the change message) allows developers to see what a fragment contains without opening it.

## Branch-based Naming (Optional)

When `IncludeBranchName` is enabled (default), the branch slug is included:
`YYYYMMDDHHmmssfff-[branch]-[slug].md`

Example: `20260624143005123-feature-add-payment-api-add-mcp-server.md`

## Slug Generation Rules
* Convert to lowercase.
* Replace spaces and special characters with hyphens `-`.
* Truncate to a reasonable length (e.g., 50 characters).
