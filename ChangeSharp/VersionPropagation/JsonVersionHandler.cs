using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChangeSharp.VersionPropagation;

public class JsonVersionHandler : IVersionPropagationHandler
{
    public bool CanHandle(VersionTargetConfig target)
    {
        if (target.Type?.Equals("json", StringComparison.OrdinalIgnoreCase) == true) return true;
        if (!string.IsNullOrEmpty(target.Type)) return false;

        var ext = Path.GetExtension(target.Path).ToLowerInvariant();
        return ext == ".json";
    }

    public void UpdateVersion(string basePath, VersionTargetConfig target, string nextVersion)
    {
        string fullPath = Path.Combine(basePath, target.Path);
        if (!File.Exists(fullPath)) return;

        string json = File.ReadAllText(fullPath);
        var node = JsonNode.Parse(json);
        if (node == null) return;

        string propertyName = target.JsonPath ?? "version";
        node[propertyName] = nextVersion;

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(fullPath, node.ToJsonString(options));
    }
}
