using Semver;

namespace ChangeSharp;

public class NextVersionComputer
{
    public static string GetNextSemanticVersion(string changeset, string currentVersion)
    {
        ChangeSet changeSet = new ChangelogParser().Parse(changeset);

        return ComputeVersion(currentVersion, changeSet);
    }

    public static string ComputeVersion(string currentVersion, ChangeSet changeSet)
    {
        string prefix = "";
        string versionString = currentVersion;
        if (currentVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            prefix = currentVersion.Substring(0, 1);
            versionString = currentVersion.Substring(1);
        }

        if (!SemVersion.TryParse(versionString, SemVersionStyles.Strict, out var version))
        {
            // Fallback for non-semver strings if possible, or just use 0.0.0
            version = new SemVersion(0);
        }

        SemVersion nextVersion;

        // Determine the next version based on the changes
        if (changeSet.Breaking.Count > 0 || changeSet.Removed.Count > 0)
        {
            if (version.IsPrerelease && version.Minor == 0 && version.Patch == 0)
                nextVersion = new SemVersion(version.Major, 0, 0);
            else
                nextVersion = new SemVersion(version.Major + 1, 0, 0);
        }
        else if (changeSet.Changed.Count > 0 || changeSet.Added.Count > 0 || changeSet.Deprecated.Count > 0)
        {
            if (version.IsPrerelease && version.Patch == 0)
                nextVersion = new SemVersion(version.Major, version.Minor, 0);
            else
                nextVersion = new SemVersion(version.Major, version.Minor + 1, 0);
        }
        else if (changeSet.Fixed.Count > 0 || changeSet.Security.Count > 0)
        {
            if (version.IsPrerelease)
                nextVersion = new SemVersion(version.Major, version.Minor, version.Patch);
            else
                nextVersion = new SemVersion(version.Major, version.Minor, version.Patch + 1);
        }
        else
        {
            nextVersion = version;
        }

        return $"{prefix}{nextVersion}";
    }

    public static string ComputePrereleaseVersion(string currentVersion, ChangeSet changeSet, string identifier, int counter)
    {
        string prefix = "";
        string versionString = currentVersion;
        if (currentVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            prefix = currentVersion.Substring(0, 1);
            versionString = currentVersion.Substring(1);
        }

        if (!SemVersion.TryParse(versionString, SemVersionStyles.Strict, out var version))
        {
            version = new SemVersion(0);
        }

        // 1. If current version is already a pre-release, we might be continuing it
        // but the spec says "base version continues to be calculated from unreleased changesets"
        // and "final version is promoted without recalculating".
        
        // We need the base version from which we bump
        SemVersion baseVersion = version;
        if (version.IsPrerelease)
        {
            baseVersion = new SemVersion(version.Major, version.Minor, version.Patch);
        }

        // Calculate what the NEXT stable version would be based on fragments
        string baseVersionString = $"{prefix}{baseVersion}";
        string nextStable = ComputeVersion(baseVersionString, changeSet);
        
        // Strip prefix from nextStable for SemVersion parsing
        string nextStableStripped = nextStable;
        if (nextStable.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            nextStableStripped = nextStable.Substring(1);
        
        var nextStableSem = SemVersion.Parse(nextStableStripped, SemVersionStyles.Strict);

        // Result is nextStable + identifier + counter
        return $"{prefix}{nextStableSem}-{identifier}.{counter}";
    }
}