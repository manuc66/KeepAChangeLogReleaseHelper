using ChangeSharp;
using Semver;

namespace ChangeSharp.Tests;

[TestFixture]
public class PrereleaseTests
{
    private string _tempPath = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "ChangeSharpPrereleaseTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempPath);
        
        var manager = new WorkspaceManager(_tempPath);
        manager.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }

    [Test]
    public void ComputePrereleaseVersion_HandlesBumpsAndCounters()
    {
        var changeSet = new ChangeSet();
        changeSet.Added.Add("New feature"); // Should trigger Minor bump

        // Current: 1.0.0 -> Next stable: 1.1.0 -> Prerelease: 1.1.0-alpha.1
        string next = NextVersionComputer.ComputePrereleaseVersion("1.0.0", changeSet, "alpha", 1);
        Assert.That(next, Is.EqualTo("1.1.0-alpha.1"));

        // Next counter: 1.1.0-alpha.2
        string next2 = NextVersionComputer.ComputePrereleaseVersion("1.0.0", changeSet, "alpha", 2);
        Assert.That(next2, Is.EqualTo("1.1.0-alpha.2"));
    }

    [Test]
    public void CreatePrerelease_IncrementsCounterIfSameBaseVersion()
    {
        var manager = new WorkspaceManager(_tempPath);
        manager.CreateFragment("Feature 1", "Added");

        string v1 = manager.CreatePrerelease(branch: "feat/test");
        Assert.That(v1, Is.EqualTo("0.1.0-feat-test.1"));

        // Run again with same fragment
        string v2 = manager.CreatePrerelease(branch: "feat/test");
        Assert.That(v2, Is.EqualTo("0.1.0-feat-test.2"));
    }

    [Test]
    public void CreatePrerelease_ResetsCounterIfBaseVersionChanges()
    {
        var manager = new WorkspaceManager(_tempPath);
        manager.CreateFragment("Bug fix", "Fixed");

        string v1 = manager.CreatePrerelease(branch: "feat/test");
        Assert.That(v1, Is.EqualTo("0.0.1-feat-test.1"));

        // Add a feature to trigger Minor bump
        manager.CreateFragment("Feature", "Added");
        
        string v2 = manager.CreatePrerelease(branch: "feat/test");
        Assert.That(v2, Is.EqualTo("0.1.0-feat-test.1"));
    }

    [Test]
    public void PromotePrerelease_UpdatesChangelogWithStableVersion()
    {
        var manager = new WorkspaceManager(_tempPath);
        manager.CreateFragment("Feature 1", "Added");
        manager.CreatePrerelease(branch: "feat/test");

        string finalVersion = manager.PromotePrerelease(branch: "feat/test");
        Assert.That(finalVersion, Is.EqualTo("0.1.0"));

        string changelog = File.ReadAllText(Path.Combine(_tempPath, "CHANGELOG.md"));
        Assert.That(changelog, Contains.Substring("## [0.1.0]"));
        Assert.That(changelog, Contains.Substring("### Added"));
        Assert.That(changelog, Contains.Substring("- Feature 1"));
        
        // Fragments should be gone
        string unreleasedDir = Path.Combine(_tempPath, ".changesharp/unreleased");
        Assert.That(Directory.GetFiles(unreleasedDir).Length, Is.EqualTo(0));
        
        // Prerelease info should be gone
        string prereleasesDir = Path.Combine(_tempPath, ".changesharp/prereleases/feat-test");
        Assert.That(Directory.Exists(prereleasesDir), Is.False);
    }
}
