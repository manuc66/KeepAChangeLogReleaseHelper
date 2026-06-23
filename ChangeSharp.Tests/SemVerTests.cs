using Semver;
using ChangeSharp;

namespace ChangeSharp.Tests;

public class SemVerTests
{
    [TestCase("1.2.3", "1.2.4", "Fixed")]
    [TestCase("1.2.3", "1.3.0", "Added")]
    [TestCase("1.2.3", "2.0.0", "Breaking Changes")]
    [TestCase("v1.2.3", "v1.2.4", "Fixed")]
    [TestCase("V1.2.3", "V1.3.0", "Added")]
    [TestCase("1.2.3-beta.1", "1.2.3", "Fixed")] // Current logic strips pre-release on bump
    [TestCase("1.2.3-alpha", "1.3.0", "Added")]
    public void ComputeVersion_HandlesPrefixesAndBumps(string current, string expected, string category)
    {
        var changeSet = new ChangeSet();
        if (category == "Breaking Changes") changeSet.Breaking.Add("Break");
        else if (category == "Added") changeSet.Added.Add("Add");
        else if (category == "Fixed") changeSet.Fixed.Add("Fix");

        string next = NextVersionComputer.ComputeVersion(current, changeSet);
        Assert.That(next, Is.EqualTo(expected));
    }

    [Test]
    public void ChangeLog_HandlesVersionWithVPrefix()
    {
        var content = @"# Changelog
## [Unreleased]
## [v1.0.0] - 2023-01-01
### Added
- Initial version
";
        var changeLog = new ChangeLog(content);
        Assert.That(changeLog.LastVersion, Is.EqualTo("v1.0.0"));

        var released = changeLog.Release(new DateTime(2023, 2, 1), "### Fixed\n- Bug fix");
        Assert.That(released.LastVersion, Is.EqualTo("v1.0.1"));
        Assert.That(released.ToString(), Contains.Substring("## [v1.0.1] - 2023-02-01"));
    }

    [Test]
    public void ChangeLog_HandlesSemVerWithPreRelease()
    {
        var content = @"# Changelog
## [Unreleased]
## [1.2.3-beta.1] - 2023-01-01
";
        var changeLog = new ChangeLog(content);
        Assert.That(changeLog.LastVersion, Is.EqualTo("1.2.3-beta.1"));

        var released = changeLog.Release(new DateTime(2023, 2, 1), "### Added\n- Feature");
        // Our current logic: 1.2.3-beta.1 + Added -> 1.3.0
        Assert.That(released.LastVersion, Is.EqualTo("1.3.0"));
    }
}
