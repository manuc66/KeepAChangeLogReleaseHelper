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

    public string? UpdateVersion(string basePath, VersionTargetConfig target, string nextVersion)
    {
        string fullPath = Path.Combine(basePath, target.Path);
        if (!File.Exists(fullPath))
            return $"JSON version target not found: {target.Path}";

        string json = File.ReadAllText(fullPath);
        var node = JsonNode.Parse(json);
        if (node == null) return $"Failed to parse JSON: {target.Path}";

        string path = target.JsonPath ?? "version";
        string? warning = SetNodeByPath(node, path, nextVersion);

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(fullPath, node.ToJsonString(options));
        return warning;
    }

    private static string? SetNodeByPath(JsonNode root, string path, string value)
    {
        // Strip optional "$." prefix (JSONPath-like)
        var segments = path.StartsWith("$.") ? path[2..].Split('.') : path.Split('.');

        string? warning = null;
        JsonNode current = root;
        for (int i = 0; i < segments.Length - 1; i++)
        {
            var seg = segments[i];
            var obj = current as JsonObject;
            if (obj == null) return $"JSON path '{path}' could not be applied: intermediate segment '{seg}' is not an object.";

            if (!obj.ContainsKey(seg))
            {
                obj[seg] = new JsonObject();
                warning ??= $"JSON path '{path}' created missing intermediate node(s).";
            }

            current = obj[seg]!;
        }

        var last = segments[^1];
        if (current is JsonObject targetObj)
        {
            targetObj[last] = value;
        }

        return warning;
    }
}
