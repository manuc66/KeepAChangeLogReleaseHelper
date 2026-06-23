using NUnit.Framework;
using System.IO;
using System.Text;

namespace ChangeSharp.Tests;

public class WorkspaceManagerTests
{
    private string _testDir;

    [SetUp]
    public void Setup()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ChangeSharpTests", Path.GetRandomFileName());
        Directory.CreateDirectory(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Test]
    public void Release_NormalWorkflow_Works()
    {
        var manager = new WorkspaceManager(_testDir);
        manager.Initialize();
        manager.CreateFragment("Added a feature", "Added");

        string nextVersion = manager.Release(DateTime.Today);

        Assert.That(nextVersion, Is.EqualTo("0.1.0"));
        Assert.That(File.Exists(Path.Combine(_testDir, "CHANGELOG.md")), Is.True);
        
        // Fragments should be deleted
        string unreleasedDir = Path.Combine(_testDir, ".changesharp/unreleased");
        Assert.That(Directory.GetFiles(unreleasedDir, "*.md").Length, Is.EqualTo(0));
        
        // Releasing dir should be empty
        string releasingDir = Path.Combine(_testDir, ".changesharp/releasing");
        Assert.That(Directory.GetFiles(releasingDir, "*.md").Length, Is.EqualTo(0));
    }

    [Test]
    public void Release_InterruptedAfterMove_ResumesCorrectly()
    {
        var manager = new WorkspaceManager(_testDir);
        manager.Initialize();
        string fragmentPath = manager.CreateFragment("Added a feature", "Added");
        
        // Simulate interruption after moving to releasing dir
        string releasingDir = Path.Combine(_testDir, ".changesharp/releasing");
        Directory.CreateDirectory(releasingDir);
        string filename = Path.GetFileName(fragmentPath);
        File.Move(fragmentPath, Path.Combine(releasingDir, filename));

        // Act
        string nextVersion = manager.Release(DateTime.Today);

        // Assert
        Assert.That(nextVersion, Is.EqualTo("0.1.0"));
        Assert.That(File.Exists(Path.Combine(_testDir, "CHANGELOG.md")), Is.True);
        Assert.That(File.ReadAllText(Path.Combine(_testDir, "CHANGELOG.md")), Contains.Substring("### Added"));
        Assert.That(File.ReadAllText(Path.Combine(_testDir, "CHANGELOG.md")), Contains.Substring("- Added a feature"));
        
        Assert.That(Directory.GetFiles(releasingDir, "*.md").Length, Is.EqualTo(0));
    }

    [Test]
    public void Release_InterruptedAfterChangelogUpdate_ResumesAndCleansUp()
    {
        var manager = new WorkspaceManager(_testDir);
        manager.Initialize();
        string fragmentPath = manager.CreateFragment("Added a feature", "Added");
        
        // Simulate interruption after CHANGELOG update but before fragment deletion
        string releasingDir = Path.Combine(_testDir, ".changesharp/releasing");
        Directory.CreateDirectory(releasingDir);
        string filename = Path.GetFileName(fragmentPath);
        File.Move(fragmentPath, Path.Combine(releasingDir, filename));

        // Pre-update CHANGELOG.md
        var releaseDate = DateTime.Today;
        string changelogPath = Path.Combine(_testDir, "CHANGELOG.md");
        string content = File.ReadAllText(changelogPath);
        var changeLog = new ChangeLog(content);
        var updated = changeLog.ReleaseWithVersion(releaseDate, "0.1.0", "### Added\n- Added a feature");
        File.WriteAllText(changelogPath, updated.ToString());

        // Act
        string nextVersion = manager.Release(releaseDate);

        // Assert
        Assert.That(nextVersion, Is.EqualTo("0.1.0"));
        // Fragments should be deleted now
        Assert.That(Directory.GetFiles(releasingDir, "*.md").Length, Is.EqualTo(0));
    }

    [Test]
    public void Release_Conflict_ThrowsException()
    {
        var manager = new WorkspaceManager(_testDir);
        manager.Initialize();
        manager.CreateFragment("Added a feature", "Added");
        
        // Pre-create version 0.1.0 with DIFFERENT content
        string changelogPath = Path.Combine(_testDir, "CHANGELOG.md");
        File.WriteAllText(changelogPath, @"# Changelog
## [0.1.0] - 2026-01-01
### Added
- Different feature
");

        // Act & Assert - force version 0.1.0 which already exists with different content
        Assert.Throws<InvalidOperationException>(() => manager.Release(DateTime.Today, forcedVersion: "0.1.0"));
    }

    [Test]
    public void Release_ChangedTriggersMajor_ByDefault()
    {
        var manager = new WorkspaceManager(_testDir);
        manager.Initialize();
        manager.CreateFragment("Changed a feature", "Changed");

        string nextVersion = manager.Release(DateTime.Today);

        Assert.That(nextVersion, Is.EqualTo("1.0.0"));
    }

    [Test]
    public void Release_SemverPolicyOverride_Works()
    {
        var manager = new WorkspaceManager(_testDir);
        manager.Initialize();
        
        // Override policy in changesharp.json
        string configPath = Path.Combine(_testDir, "changesharp.json");
        string json = File.ReadAllText(configPath);
        var config = System.Text.Json.JsonSerializer.Deserialize<ChangeSharpConfig>(json);
        config.SemverPolicy.Changed = "Minor";
        File.WriteAllText(configPath, System.Text.Json.JsonSerializer.Serialize(config));
        
        manager.CreateFragment("Changed a feature", "Changed");

        string nextVersion = manager.Release(DateTime.Today);

        Assert.That(nextVersion, Is.EqualTo("0.1.0"));
    }
}
