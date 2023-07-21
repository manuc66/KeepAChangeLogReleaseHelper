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
        if (changeSet.HasMajor)
        {
            version = new Version(version.Major + 1, 0, 0);
        }
        else if (changeSet.HasMinor)
        {
            version = new Version(version.Major, version.Minor + 1, 0);
        }
        else if (changeSet.HasPatch)
        {
            version = new Version(version.Major, version.Minor, version.Build + 1);
        }

        return version.ToString();
    }
}