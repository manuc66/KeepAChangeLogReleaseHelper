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
        ## Changed
        - Improved existing feature 1.

        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("2.0.0"));
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
    public void ItComputeAMajorrFromRemoved()
    {
        string changeset = @"

        ## Removed
        - Removed feature 2.
        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("2.0.0"));
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


    [Test]
    public void ItComputeAMinorIfChangedANdRemovedAreEmpty()
    {
        string changeset = @"
        ## Added
        - New feature 1.
        - New feature 2.

        ## Changed

        ## Deprecated
        - Deprecated feature 1.

        ## Removed

        ## Fixed
        - Bug fix 1.
        - Bug fix 2.

        ## Security
        - Security update 1.
        
        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.1.0"));
    }

    [Test]
    public void ItComputeAFixIfOnlyFixedAndSecurityAreFilled()
    {
        string changeset = @"
        ## Added

        ## Changed

        ## Deprecated

        ## Removed

        ## Fixed
        - Bug fix 1.
        - Bug fix 2.

        ## Security
        - Security update 1.
        
        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.0.1"));
    }

    [Test]
    public void ItComputeAPathIfOnlyFixedIsFilled()
    {
        string changeset = @"
        ## Added

        ## Changed

        ## Deprecated

        ## Removed

        ## Fixed
        - Bug fix 1.
        - Bug fix 2.

        ## Security
        
        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.0.1"));
    }

    [Test]
    public void ItComputeAPathIfOnlySecurityIsFilled()
    {
        string changeset = @"
        ## Added

        ## Changed

        ## Deprecated

        ## Removed

        ## Fixed

        ## Security
        - Security update 1.
        
        ";

        string nextVersion = GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.0.1"));
    }

    class ReleaseChangeLog
    {
        public bool HasMinor { get; set; }
        public bool HasMajor { get; set; }
        public bool HasPatch { get; set; }
    }

    class ChangelogParser
    {
        public ChangelogParser()
        {
        }

        internal ReleaseChangeLog Parse(string changeset)
        {
            // Split changeset into lines
            IEnumerable<string> lines = changeset.Split('\n').Select(line => line.Trim());

            bool hasMajor = false;
            bool hasMinor = false;
            bool hasPatch = false;
            bool changedStarted = false;
            bool addedStarted = false;
            bool removedStarted = false;
            bool deprecatedStarted = false;
            bool fixedStarted = false;
            bool securityStarted = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("## Changed", StringComparison.OrdinalIgnoreCase))
                {
                    changedStarted = true;
                    addedStarted = false;
                    removedStarted = false;
                    deprecatedStarted = false;
                }
                else if (line.StartsWith("## Added", StringComparison.OrdinalIgnoreCase))
                {
                    changedStarted = false;
                    addedStarted = true;
                    removedStarted = false;
                    deprecatedStarted = false;
                }
                else if (line.StartsWith("## Removed", StringComparison.OrdinalIgnoreCase))
                {
                    changedStarted = false;
                    addedStarted = false;
                    removedStarted = true;
                    deprecatedStarted = false;
                }
                else if (line.StartsWith("## Deprecated", StringComparison.OrdinalIgnoreCase))
                {
                    changedStarted = false;
                    addedStarted = false;
                    removedStarted = false;
                    deprecatedStarted = true;
                }
                else if (line.StartsWith("## Fixed", StringComparison.OrdinalIgnoreCase))
                {
                    changedStarted = false;
                    addedStarted = false;
                    removedStarted = false;
                    deprecatedStarted = false;
                    fixedStarted = true;
                    securityStarted = false;
                }
                else if (line.StartsWith("## Security", StringComparison.OrdinalIgnoreCase))
                {
                    changedStarted = false;
                    addedStarted = false;
                    removedStarted = false;
                    deprecatedStarted = false;
                    fixedStarted = false;
                    securityStarted = true;
                }
                else if (line.StartsWith("## ", StringComparison.OrdinalIgnoreCase))
                {
                    changedStarted = false;
                    addedStarted = false;
                    removedStarted = false;
                    deprecatedStarted = false;
                    fixedStarted = false;
                    securityStarted = false;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (changedStarted || removedStarted)
                        {
                            hasMajor = true;
                        }

                        if (addedStarted || deprecatedStarted)
                        {
                            hasMinor = true;
                        }

                        if (fixedStarted || securityStarted)
                        {
                            hasPatch = true;
                        }
                    }
                }
            }

            return new ReleaseChangeLog()
            {
                HasMajor = hasMajor,
                HasMinor = hasMinor,
                HasPatch = hasPatch,
            };
        }
    }

    static string GetNextSemanticVersion(string changeset, string currentVersion)
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