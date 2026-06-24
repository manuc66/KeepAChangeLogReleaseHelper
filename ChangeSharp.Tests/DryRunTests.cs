using ChangeSharp;

namespace ChangeSharp.Tests;

[TestFixture]
public class DryRunTests
{
    private string _tempPath = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "ChangeSharpDryRunTests_" + Guid.NewGuid());
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
    public void Release_InDryRunMode_DoesNotModifyFiles()
    {
        var manager = new WorkspaceManager(_tempPath);
        
        // 1. Create a fragment
        manager.CreateFragment("New feature", "Added");
        
        string changelogPath = Path.Combine(_tempPath, "CHANGELOG.md");
        string originalChangelog = File.ReadAllText(changelogPath);
        
        string unreleasedDir = Path.Combine(_tempPath, ".changesharp/unreleased");
        var originalFragments = Directory.GetFiles(unreleasedDir);
        Assert.That(originalFragments.Length, Is.EqualTo(1));

        // 2. Perform Dry Run Release
        var (nextVersion, _) = manager.Release(DateTime.Today, dryRun: true);
        
        // 3. Verify files are NOT changed
        Assert.That(nextVersion, Is.EqualTo("0.1.0"));
        Assert.That(File.ReadAllText(changelogPath), Is.EqualTo(originalChangelog));
        Assert.That(Directory.GetFiles(unreleasedDir).Length, Is.EqualTo(1));
    }

    [Test]
    public void Release_InDryRunMode_DoesNotPropagateVersion()
    {
        var manager = new WorkspaceManager(_tempPath);
        
        // Create a dummy project file
        string csprojPath = Path.Combine(_tempPath, "test.csproj");
        string originalCsproj = "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><Version>1.0.0</Version></PropertyGroup></Project>";
        File.WriteAllText(csprojPath, originalCsproj);
        
        // Update config to include this target
        string configPath = Path.Combine(_tempPath, "changesharp.json");
        File.WriteAllText(configPath, "{\"VersionTargets\": [{\"Path\": \"test.csproj\"}]}");

        // Create a fragment to trigger a bump
        manager.CreateFragment("Fix bug", "Fixed");

        // Perform Dry Run Release
        manager.Release(DateTime.Today, dryRun: true);

        // Verify csproj is NOT changed
        Assert.That(File.ReadAllText(csprojPath), Is.EqualTo(originalCsproj));
    }
}
