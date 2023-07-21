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

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

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

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.1.0"));
    }

    [Test]
    public void ItComputeAMajorrFromRemoved()
    {
        string changeset = @"

        ## Removed
        - Removed feature 2.
        ";

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("2.0.0"));
    }

    [Test]
    public void ItComputeAPatchFromOnlySecurity()
    {
        string changeset = @"

        ## Security
        - Security update 1.
        ";

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

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

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

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

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

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

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

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

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

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

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

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

        string nextVersion = NextVersionComputer.GetNextSemanticVersion(changeset, "1.0.0");

        Assert.That(nextVersion, Is.EqualTo("1.0.1"));
    }
}