namespace ChangeSharp;

public class VersionTargetConfig
{
    public string Path { get; set; } = string.Empty;
    public string? Type { get; set; } // msbuild, json, regex
    public string? Regex { get; set; }
    public string? JsonPath { get; set; } // e.g. "version"
}
