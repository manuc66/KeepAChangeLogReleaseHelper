namespace KeepAChangeLogReleaseHelper;

public class NextVersionComputer
{
    public static string GetNextSemanticVersion(string changeset, string currentVersion)
    {
        ChangeSet changeSet = new ChangelogParser().Parse(changeset);

        return ComputeVersion(currentVersion, changeSet);
    }

    private static string ComputeVersion(string currentVersion, ChangeSet changeSet)
    {
        // Parse the current version
        Version version = new Version(currentVersion);

        // Determine the next version based on the changes
        if (changeSet.Changed.Count > 0 || changeSet.Removed.Count > 0)
        {
            version = new Version(version.Major + 1, 0, 0);
        }
        else if (changeSet.Added.Count > 0 || changeSet.Deprecated.Count > 0)
        {
            version = new Version(version.Major, version.Minor + 1, 0);
        }
        else if (changeSet.Fixed.Count > 0 || changeSet.Security.Count > 0)
        {
            version = new Version(version.Major, version.Minor, version.Build + 1);
        }

        return version.ToString();
    }
}