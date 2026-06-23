namespace ChangeSharp;

public class ChangeSharpConfig
{
    public string ChangelogPath { get; set; } = "CHANGELOG.md";
    public string UnreleasedDir { get; set; } = ".changesharp/unreleased";
    public string PrereleasesDir { get; set; } = ".changesharp/prereleases";
    public List<VersionTargetConfig> VersionTargets { get; set; } = new();
    public PreReleaseConfig PreRelease { get; set; } = new();
}

public class PreReleaseConfig
{
    public bool Enabled { get; set; } = true;
    public bool BranchAsIdentifier { get; set; } = true;
    public bool SanitizeBranchName { get; set; } = true;
    public int MaxIdentifierLength { get; set; } = 30;
}
