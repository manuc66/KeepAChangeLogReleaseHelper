# Fragment Naming Strategy

The convention for naming unreleased fragment files is designed for readability, easy sorting, and collision avoidance.

## Standard Format

Fragments follow this pattern:
`YYYYMMDD-HHmmss-[slug].md`

Example: `20250623-143005-add-mcp-server.md`

### Why this format?
1. **Chronological Sorting**: Timestamps ensure that files are listed in the order they were created when looking at the directory.
2. **Collision Avoidance**: The inclusion of hours, minutes, and seconds makes it extremely unlikely for two fragments to have the same name, even in multi-developer environments.
3. **Contextual Readability**: The `slug` (derived from the change message) allows developers to see what a fragment contains without opening it.

## Branch-based Naming (Optional)

In some workflows, using the Git branch name as a slug can be useful:
`YYYYMMDD-[branch-name].md`

`changesharp new` will automatically generate a slug from the provided message if no specific name is given.

## Slug Generation Rules
* Convert to lowercase.
* Replace spaces and special characters with hyphens `-`.
* Truncate to a reasonable length (e.g., 50 characters).
