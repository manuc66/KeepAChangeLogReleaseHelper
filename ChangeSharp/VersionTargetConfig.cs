namespace ChangeSharp;

public class VersionTargetConfig
{
    public string Path { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Regex { get; set; }
    public string? JsonPath { get; set; }
    public string? Replacement { get; set; }
}
