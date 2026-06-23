using System.Text.RegularExpressions;

namespace ChangeSharp.VersionPropagation;

public class RegexVersionHandler : IVersionPropagationHandler
{
    public bool CanHandle(VersionTargetConfig target)
    {
        return target.Type?.Equals("regex", StringComparison.OrdinalIgnoreCase) == true || 
               !string.IsNullOrEmpty(target.Regex);
    }

    public void UpdateVersion(string basePath, VersionTargetConfig target, string nextVersion)
    {
        string fullPath = Path.Combine(basePath, target.Path);
        if (!File.Exists(fullPath)) return;

        if (string.IsNullOrEmpty(target.Regex)) return;

        string content = File.ReadAllText(fullPath);
        
        // Use Regex.Replace. 
        // If the user wants to replace only a part of the match, they should use lookbehind/lookahead
        // or we could support a replacement pattern if we wanted to be more complex.
        // For now, let's keep it simple: it replaces the whole match.
        
        string updated = Regex.Replace(content, target.Regex, nextVersion);
        File.WriteAllText(fullPath, updated);
    }
}
