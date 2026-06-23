# Frictionless Merge Workflow

One of the primary goals of ChangeSharp is to eliminate merge conflicts in management files (like `CHANGELOG.md` and version files) when working in multi-developer environments.

## The Problem: Traditional Versioning

In traditional workflows, if Branch A and Branch B both update the version to `1.1.0` and add an entry to the top of `CHANGELOG.md`, merging them into `main` causes a conflict. The developer must manually resolve the version number and reorder the changelog entries.

## The Solution: Fragment-Based Deferred Versioning

ChangeSharp avoids this by using two core principles:

### 1. Isolated Fragments
Instead of editing `CHANGELOG.md` directly, each feature branch creates a **fragment** in `.changesharp/unreleased/`.
- **Unique Naming**: Fragments are named using a timestamp and a slug (e.g., `20250623-143005-add-feature.md`).
- **No Overlaps**: Since each branch creates its own file, Git can merge them without any conflicts.
- **Context Preservation**: The fragment stays with the feature branch and only gets "consumed" during a release.

### 2. Deferred Versioning
Version numbers in `.csproj`, `package.json`, and the `CHANGELOG.md` are **not updated** in the feature branch.
- **Release at Merge Time**: The `changesharp release` command is typically run only on the `main` branch (or via CI/CD after merge).
- **Automatic Aggregation**: When `release` is run, ChangeSharp aggregates all merged fragments, computes the correct version bump based on the *sum* of all changes, and updates the management files in a single, atomic operation.

## Best Practices for Zero Conflicts

1. **Never edit `CHANGELOG.md` manually** on a feature branch. Use `changesharp new`.
2. **Never bump versions manually** in your project files. Let `changesharp release` handle it.
3. **Commit your fragments**. They are the source of truth for your changes.
4. **Perform releases on a stable branch**. Running `release` on `main` ensures that all merged features are accounted for and that the version history remains linear and conflict-free.

## Enhanced Collision Avoidance

To further ensure that fragments never clash (even if two developers create a fragment at the exact same second), ChangeSharp can be configured to include the Git branch name in the fragment filename:

```json
{
  "FragmentNaming": {
    "IncludeBranchName": true
  }
}
```

This makes the filename: `YYYYMMDD-HHmmss-[branch]-[slug].md`.

## What about `changesharp.json`?

You might wonder if the management file itself (`changesharp.json`) could be a source of conflicts. 

The answer is **rarely**, and here is why:

1. **Configuration vs. State**: Unlike a traditional `CHANGELOG.md` which changes with every commit, `changesharp.json` is a **configuration** file. It only changes when you add a new project to the workspace or change a versioning policy.
2. **Deterministic Layout**: ChangeSharp automatically **sorts** the version targets by path. If Branch A adds `ProjectA.csproj` and Branch B adds `ProjectB.csproj`, they will be added at different locations in the JSON file, allowing Git to merge them automatically in most cases.
3. **No Incremental State**: The "state" of your pending release is stored in the `unreleased/` directory, not in the JSON file. This is the key to avoiding the "version race" common in other tools.
