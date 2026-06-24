using System.Text.RegularExpressions;

namespace ChangeSharp.VersionPropagation;

public class RegexVersionHandler : IVersionPropagationHandler
{
    public bool CanHandle(VersionTargetConfig target)
    {
        return target.Type?.Equals("regex", StringComparison.OrdinalIgnoreCase) == true || 
               !string.IsNullOrEmpty(target.Regex);
    }

    public string? UpdateVersion(string basePath, VersionTargetConfig target, string nextVersion)
    {
        string fullPath = Path.Combine(basePath, target.Path);
        if (!File.Exists(fullPath))
            return $"Regex version target not found: {target.Path}";

        if (string.IsNullOrEmpty(target.Regex))
            return $"Regex target '{target.Path}' has no regex pattern configured.";

        string content = File.ReadAllText(fullPath);
        string updated = Regex.Replace(content, target.Regex, nextVersion);
        File.WriteAllText(fullPath, updated);
        return null;
    }
}
