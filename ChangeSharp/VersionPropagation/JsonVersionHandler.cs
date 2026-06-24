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

        string path = target.JsonPath ?? "version";
        SetNodeByPath(node, path, nextVersion);

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(fullPath, node.ToJsonString(options));
    }

    private static void SetNodeByPath(JsonNode root, string path, string value)
    {
        // Strip optional "$." prefix (JSONPath-like)
        var segments = path.StartsWith("$.") ? path[2..].Split('.') : path.Split('.');

        JsonNode current = root;
        for (int i = 0; i < segments.Length - 1; i++)
        {
            var seg = segments[i];
            var obj = current as JsonObject;
            if (obj == null) return;

            if (!obj.ContainsKey(seg))
                obj[seg] = new JsonObject();

            current = obj[seg]!;
        }

        var last = segments[^1];
        if (current is JsonObject targetObj)
        {
            targetObj[last] = value;
        }
    }
}
