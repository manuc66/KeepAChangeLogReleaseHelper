using System.Text.Json.Serialization;

namespace ChangeSharp;

public class VersionTargetConfig
{
    public string Path { get; set; } = string.Empty;
    public string? Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Regex { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? JsonPath { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Replacement { get; set; }
}
