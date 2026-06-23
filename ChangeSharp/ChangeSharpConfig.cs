namespace ChangeSharp;

public class ChangeSharpConfig
{
    public string ChangelogPath { get; set; } = "CHANGELOG.md";
    public string UnreleasedDir { get; set; } = ".changesharp/unreleased";
    public string ReleasingDir { get; set; } = ".changesharp/releasing";
    public string PrereleasesDir { get; set; } = ".changesharp/prereleases";
    public List<VersionTargetConfig> VersionTargets { get; set; } = new();
    public PreReleaseConfig PreRelease { get; set; } = new();
    public SemverPolicyConfig SemverPolicy { get; set; } = new();
}

public class SemverPolicyConfig
{
    public string Breaking { get; set; } = "Major";
    public string Removed { get; set; } = "Major";
    public string Changed { get; set; } = "Major";
    public string Added { get; set; } = "Minor";
    public string Deprecated { get; set; } = "Minor";
    public string Fixed { get; set; } = "Patch";
    public string Security { get; set; } = "Patch";
}

public class PreReleaseConfig
{
    public bool Enabled { get; set; } = true;
    public bool BranchAsIdentifier { get; set; } = true;
    public bool SanitizeBranchName { get; set; } = true;
    public int MaxIdentifierLength { get; set; } = 30;
}
