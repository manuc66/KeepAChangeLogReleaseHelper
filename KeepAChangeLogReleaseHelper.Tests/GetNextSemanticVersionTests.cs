namespace KeepAChangeLogReleaseHelper.Tests;

public class GetNextSemanticVersionTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ItComputeAMajorFromChanged()
    {
        string changeset = @"
        - Improved existing feature 1.

        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.1.0"));
    }
    
    [Test]
    public void ItComputeAMinorFromAdded()
    {
        string changeset = @"
        ## Added
        - New feature 1.
        - New feature 2.

        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.1.0"));
    }

    [Test]
    public void ItComputeAMinorFromRemoved()
    {
        string changeset = @"

        ## Removed
        - Removed feature 2.
        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.1.0"));
    }

    [Test]
    public void ItComputeAPatchFromOnlySecurity()
    {
        string changeset = @"

        ## Security
        - Security update 1.
        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.0.1"));
    }

    [Test]
    public void ItComputeAPatchFromOnlyFixes()
    {
        string changeset = @"
        ## Fixed
        - Bug fix 1.
        - Bug fix 2.
";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.0.1"));
    }

    [Test]
    public void ItComputeAMajorFromAFullChangeSet()
    {
        string changeset = @"
        ## Added
        - New feature 1.
        - New feature 2.

        ## Changed
        - Improved existing feature 1.

        ## Deprecated
        - Deprecated feature 1.

        ## Removed
        - Removed feature 2.

        ## Fixed
        - Bug fix 1.
        - Bug fix 2.

        ## Security
        - Security update 1.
        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("2.0.0"));
    }

    static string GetNextSemanticVersion(string changeset, string currentVersion)
    {
        // Split changeset into lines
        var lines = changeset.Split('\n').Select(line => line.Trim());

        // Check for changes in each category
        bool major = lines.Any(line => line.StartsWith("## Changed", StringComparison.OrdinalIgnoreCase));
        bool minor = lines.Any(line => line.StartsWith("## Added", StringComparison.OrdinalIgnoreCase) || line.StartsWith("## Removed", StringComparison.OrdinalIgnoreCase) ||
                                       line.StartsWith("## Deprecated", StringComparison.OrdinalIgnoreCase));
        bool patch = lines.Any(line => line.StartsWith("## Fixed", StringComparison.OrdinalIgnoreCase) || line.StartsWith("## Security", StringComparison.OrdinalIgnoreCase));

        // Parse the current version
        Version version = new Version(currentVersion);

        // Determine the next version based on the changes
        if (major)
        {
            version = new Version(version.Major + 1, 0, 0);
        }
        else if (minor)
        {
            version = new Version(version.Major, version.Minor + 1, 0);
        }
        else if (patch)
        {
            version = new Version(version.Major, version.Minor, version.Build + 1);
        }

        return version.ToString();
    }
}