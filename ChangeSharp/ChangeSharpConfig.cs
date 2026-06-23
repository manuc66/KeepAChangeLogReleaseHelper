namespace ChangeSharp;

public class ChangeSharpConfig
{
    public string ChangelogPath { get; set; } = "CHANGELOG.md";
    public string UnreleasedDir { get; set; } = ".changesharp/unreleased";
    public List<string> VersionTargets { get; set; } = new();
}
