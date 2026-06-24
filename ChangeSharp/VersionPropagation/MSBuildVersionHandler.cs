using System.Xml.Linq;

namespace ChangeSharp.VersionPropagation;

public class MSBuildVersionHandler : IVersionPropagationHandler
{
    public bool CanHandle(VersionTargetConfig target)
    {
        if (target.Type?.Equals("msbuild", StringComparison.OrdinalIgnoreCase) == true) return true;
        if (!string.IsNullOrEmpty(target.Type)) return false;

        var ext = Path.GetExtension(target.Path).ToLowerInvariant();
        return ext == ".csproj" || ext == ".props" || ext == ".targets";
    }

    public void UpdateVersion(string basePath, VersionTargetConfig target, string nextVersion)
    {
        string fullPath = Path.Combine(basePath, target.Path);
        if (!File.Exists(fullPath)) return;

        XDocument doc;
        using (var stream = File.OpenRead(fullPath))
        {
            doc = XDocument.Load(stream);
        }

        var versionElements = doc.Descendants("Version").ToList();
        var versionPrefixElements = doc.Descendants("VersionPrefix").ToList();

        if (!versionElements.Any() && !versionPrefixElements.Any())
        {
            var propertyGroup = doc.Descendants("PropertyGroup").FirstOrDefault();
            if (propertyGroup != null)
            {
                propertyGroup.Add(new XElement("Version", nextVersion));
            }
            else
            {
                var project = doc.Root;
                if (project != null)
                {
                    project.Add(new XElement("PropertyGroup", new XElement("Version", nextVersion)));
                }
            }
        }
        else if (versionElements.Any())
        {
            // Prefer <Version> — it takes precedence and avoids breaking MinVer-style VersionPrefix flows
            foreach (var el in versionElements) el.Value = nextVersion;
        }
        else
        {
            // Fallback to <VersionPrefix> if only that exists
            foreach (var el in versionPrefixElements) el.Value = nextVersion;
        }

        doc.Save(fullPath);
    }
}
