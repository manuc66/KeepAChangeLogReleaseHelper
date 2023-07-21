namespace KeepAChangeLogReleaseHelper.Tests;

public class NextVersionComputer
{
    public static string GetNextSemanticVersion(string changeset, string currentVersion)
    {
        ReleaseChangeLog releaseChangeLog = new ChangelogParser().Parse(changeset);

        return ComputeVersion(currentVersion, releaseChangeLog);
    }

    private static string ComputeVersion(string currentVersion, ReleaseChangeLog releaseChangeLog)
    {
        // Parse the current version
        Version version = new Version(currentVersion);

        // Determine the next version based on the changes
        if (releaseChangeLog.HasMajor)
        {
            version = new Version(version.Major + 1, 0, 0);
        }
        else if (releaseChangeLog.HasMinor)
        {
            version = new Version(version.Major, version.Minor + 1, 0);
        }
        else if (releaseChangeLog.HasPatch)
        {
            version = new Version(version.Major, version.Minor, version.Build + 1);
        }

        return version.ToString();
    }
}