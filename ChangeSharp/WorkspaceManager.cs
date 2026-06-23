using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChangeSharp;

public class WorkspaceManager
{
    private readonly string _basePath;
    private const string ConfigFileName = "changesharp.json";

    public WorkspaceManager(string? basePath = null)
    {
        _basePath = basePath ?? Directory.GetCurrentDirectory();
    }

    private string ConfigFilePath => Path.Combine(_basePath, ConfigFileName);

    public void Initialize()
    {
        // 1. Create default configuration
        var config = new ChangeSharpConfig();
        if (!File.Exists(ConfigFilePath))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigFilePath, json, Encoding.UTF8);
        }
        else
        {
            config = LoadConfig();
        }

        // 2. Create unreleased directory
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        if (!Directory.Exists(unreleasedPath))
        {
            Directory.CreateDirectory(unreleasedPath);
        }

        // 3. Create default CHANGELOG.md if it doesn't exist
        string changelogPath = Path.Combine(_basePath, config.ChangelogPath);
        if (!File.Exists(changelogPath))
        {
            string defaultChangelog = @"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
";
            File.WriteAllText(changelogPath, defaultChangelog, Encoding.UTF8);
        }
    }

    public string CreateFragment(string message, string category)
    {
        var config = LoadConfig();
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        if (!Directory.Exists(unreleasedPath))
        {
            Directory.CreateDirectory(unreleasedPath);
        }

        // Clean/slugify message for filename
        string slug = Slugify(message);
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string filename = $"{timestamp}-{slug}.md";
        string filePath = Path.Combine(unreleasedPath, filename);

        // Normalize category naming (e.g. "Breaking Changes" vs "Added")
        string formattedCategory = NormalizeCategory(category);

        string content = $"### {formattedCategory}{Environment.NewLine}- {message}{Environment.NewLine}";
        File.WriteAllText(filePath, content, Encoding.UTF8);

        return filePath;
    }

    public void GetStatus(
        out int fragmentCount,
        out ChangeSet mergedChangeSet,
        out string currentVersion,
        out string nextVersion)
    {
        var config = LoadConfig();
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        
        var fragments = Directory.Exists(unreleasedPath)
            ? Directory.GetFiles(unreleasedPath, "*.md")
            : Array.Empty<string>();

        fragmentCount = fragments.Length;

        var parser = new ChangelogParser();
        var changeSets = new List<ChangeSet>();

        foreach (var file in fragments)
        {
            string fileContent = File.ReadAllText(file, Encoding.UTF8);
            changeSets.Add(parser.Parse(fileContent));
        }

        mergedChangeSet = changeSets.Count > 0
            ? changeSets.Aggregate(new ChangeSet(), (a, b) => a.Merge(b))
            : new ChangeSet();

        // Load current version from CHANGELOG.md
        string changelogPath = Path.Combine(_basePath, config.ChangelogPath);
        if (File.Exists(changelogPath))
        {
            string changelogContent = File.ReadAllText(changelogPath, Encoding.UTF8);
            var changeLog = new ChangeLog(changelogContent);
            currentVersion = changeLog.LastVersion;
        }
        else
        {
            currentVersion = "0.0.0";
        }

        nextVersion = NextVersionComputer.ComputeVersion(currentVersion, mergedChangeSet);
    }

    public string Release(DateTime releaseDate)
    {
        var config = LoadConfig();
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        string changelogPath = Path.Combine(_basePath, config.ChangelogPath);

        GetStatus(out int fragmentCount, out ChangeSet mergedChangeSet, out string currentVersion, out string nextVersion);

        if (fragmentCount == 0)
        {
            throw new InvalidOperationException("No unreleased fragments found to release.");
        }

        string changelogContent = File.Exists(changelogPath)
            ? File.ReadAllText(changelogPath, Encoding.UTF8)
            : @"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
";

        var changeLog = new ChangeLog(changelogContent);
        // Release creates the new version segment and updates Unreleased section
        var updatedChangelog = changeLog.Release(releaseDate, mergedChangeSet.ToChangelogString());

        // Save updated changelog
        File.WriteAllText(changelogPath, updatedChangelog.ToString(), Encoding.UTF8);

        // Delete processed unreleased fragments
        var fragments = Directory.GetFiles(unreleasedPath, "*.md");
        foreach (var file in fragments)
        {
            File.Delete(file);
        }

        return nextVersion;
    }

    public ChangeSharpConfig LoadConfig()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new ChangeSharpConfig();
        }

        try
        {
            string json = File.ReadAllText(ConfigFilePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<ChangeSharpConfig>(json) ?? new ChangeSharpConfig();
        }
        catch
        {
            return new ChangeSharpConfig();
        }
    }

    private static string Slugify(string text)
    {
        string normalized = text.ToLowerInvariant();
        // Replace non-alphanumeric characters with hyphens
        normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", "");
        // Replace multiple spaces/hyphens with a single hyphen
        normalized = Regex.Replace(normalized, @"[\s-]+", "-").Trim('-');
        
        if (normalized.Length > 50)
        {
            normalized = normalized.Substring(0, 50).Trim('-');
        }

        return string.IsNullOrEmpty(normalized) ? "change" : normalized;
    }

    private static string NormalizeCategory(string category)
    {
        string cleaned = category.Trim();
        if (cleaned.Equals("breaking", StringComparison.OrdinalIgnoreCase) ||
            cleaned.Equals("breaking changes", StringComparison.OrdinalIgnoreCase))
        {
            return "Breaking Changes";
        }

        // Title case for standard Keep a Changelog categories
        if (cleaned.Equals("added", StringComparison.OrdinalIgnoreCase)) return "Added";
        if (cleaned.Equals("changed", StringComparison.OrdinalIgnoreCase)) return "Changed";
        if (cleaned.Equals("deprecated", StringComparison.OrdinalIgnoreCase)) return "Deprecated";
        if (cleaned.Equals("removed", StringComparison.OrdinalIgnoreCase)) return "Removed";
        if (cleaned.Equals("fixed", StringComparison.OrdinalIgnoreCase)) return "Fixed";
        if (cleaned.Equals("security", StringComparison.OrdinalIgnoreCase)) return "Security";

        return cleaned; // Fallback
    }
}
