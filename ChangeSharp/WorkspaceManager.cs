using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChangeSharp.VersionPropagation;

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

        // 2b. Create prereleases directory
        string prereleasesPath = Path.Combine(_basePath, config.PrereleasesDir);
        if (!Directory.Exists(prereleasesPath))
        {
            Directory.CreateDirectory(prereleasesPath);
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

    public string Release(DateTime releaseDate, bool dryRun = false, string? forcedVersion = null)
    {
        var config = LoadConfig();
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        string changelogPath = Path.Combine(_basePath, config.ChangelogPath);

        GetStatus(out int fragmentCount, out ChangeSet mergedChangeSet, out string currentVersion, out string nextVersion);

        if (forcedVersion != null) nextVersion = forcedVersion;

        if (fragmentCount == 0)
        {
            throw new InvalidOperationException("No unreleased fragments found to release.");
        }

        if (dryRun)
        {
            return nextVersion;
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
        var updatedChangelog = changeLog.ReleaseWithVersion(releaseDate, forcedVersion, mergedChangeSet.ToChangelogString());

        // Save updated changelog
        File.WriteAllText(changelogPath, updatedChangelog.ToString(), Encoding.UTF8);

        // Delete processed unreleased fragments
        var fragments = Directory.GetFiles(unreleasedPath, "*.md");
        foreach (var file in fragments)
        {
            File.Delete(file);
        }

        // Propagate version to targets
        PropagateVersion(config, nextVersion);

        return nextVersion;
    }

    public IEnumerable<string> GetEffectiveVersionTargets()
    {
        var config = LoadConfig();
        var handlers = GetHandlers();
        
        foreach (var target in config.VersionTargets)
        {
            var handler = handlers.FirstOrDefault(h => h.CanHandle(target));
            if (handler != null)
            {
                yield return target.Path;
            }
        }
    }

    private void PropagateVersion(ChangeSharpConfig config, string nextVersion)
    {
        var handlers = GetHandlers();

        foreach (var target in config.VersionTargets)
        {
            var handler = handlers.FirstOrDefault(h => h.CanHandle(target));
            if (handler != null)
            {
                handler.UpdateVersion(_basePath, target, nextVersion);
            }
        }
    }

    private static List<IVersionPropagationHandler> GetHandlers()
    {
        return new List<IVersionPropagationHandler>
        {
            new MSBuildVersionHandler(),
            new JsonVersionHandler(),
            new RegexVersionHandler()
        };
    }

    public string CreatePrerelease(string? branch = null, string? channel = null, bool dryRun = false)
    {
        var config = LoadConfig();
        string branchName = branch ?? GetCurrentBranch();
        string branchSlug = SanitizeBranchName(branchName);
        
        string identifier = branchSlug;
        if (!string.IsNullOrEmpty(channel))
        {
            identifier = $"{branchSlug}.{channel}";
        }

        GetStatus(out int count, out ChangeSet merged, out string current, out string nextStable);

        if (count == 0)
        {
            throw new InvalidOperationException("No unreleased fragments found to create a pre-release.");
        }

        // Determine next counter
        int counter = 1;
        string prereleaseDir = Path.Combine(_basePath, config.PrereleasesDir, branchSlug);
        string infoPath = Path.Combine(prereleaseDir, "info.json");

        if (File.Exists(infoPath))
        {
            try
            {
                string json = File.ReadAllText(infoPath, Encoding.UTF8);
                var info = JsonSerializer.Deserialize<PrereleaseInfo>(json);
                if (info != null)
                {
                    // If the base version is the same, increment counter
                    // If base version increased (more changes), reset counter? 
                    // Actually, the spec examples show subsequent executions incrementing the counter.
                    // Let's check if the base version changed.
                    if (info.BaseVersion == nextStable)
                    {
                        counter = info.Counter + 1;
                    }
                    else
                    {
                        // New base version, reset counter
                        counter = 1;
                    }
                }
            }
            catch { /* Ignore corrupt info */ }
        }

        string nextPrerelease = NextVersionComputer.ComputePrereleaseVersion(current, merged, identifier, counter);

        if (dryRun) return nextPrerelease;

        // Save info
        if (!Directory.Exists(prereleaseDir)) Directory.CreateDirectory(prereleaseDir);
        var newInfo = new PrereleaseInfo
        {
            Version = nextPrerelease,
            BaseVersion = nextStable,
            Counter = counter,
            Branch = branchName,
            Timestamp = DateTime.UtcNow
        };
        string newJson = JsonSerializer.Serialize(newInfo, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(infoPath, newJson, Encoding.UTF8);

        return nextPrerelease;
    }

    public List<PrereleaseInfo> ListPrereleases()
    {
        var config = LoadConfig();
        string prereleasesPath = Path.Combine(_basePath, config.PrereleasesDir);
        var result = new List<PrereleaseInfo>();

        if (!Directory.Exists(prereleasesPath)) return result;

        foreach (var dir in Directory.GetDirectories(prereleasesPath))
        {
            string infoPath = Path.Combine(dir, "info.json");
            if (File.Exists(infoPath))
            {
                try
                {
                    string json = File.ReadAllText(infoPath, Encoding.UTF8);
                    var info = JsonSerializer.Deserialize<PrereleaseInfo>(json);
                    if (info != null) result.Add(info);
                }
                catch { }
            }
        }

        return result.OrderByDescending(x => x.Timestamp).ToList();
    }

    public string PromotePrerelease(string? branch = null, bool dryRun = false)
    {
        var config = LoadConfig();
        string branchName = branch ?? GetCurrentBranch();
        string branchSlug = SanitizeBranchName(branchName);
        
        string prereleaseDir = Path.Combine(_basePath, config.PrereleasesDir, branchSlug);
        string infoPath = Path.Combine(prereleaseDir, "info.json");

        if (!File.Exists(infoPath))
        {
            throw new InvalidOperationException($"No active pre-release found for branch '{branchName}'.");
        }

        string json = File.ReadAllText(infoPath, Encoding.UTF8);
        var info = JsonSerializer.Deserialize<PrereleaseInfo>(json);
        if (info == null) throw new InvalidOperationException("Failed to load pre-release info.");

        // Promotion uses the base version (final stable version)
        string finalVersion = info.BaseVersion;

        if (dryRun) return finalVersion;

        // Use the Release method but we need to ensure it uses the correct version
        // Actually, Release() recalculates the version. 
        // "The final version is promoted without recalculating the version number."
        // We might need a variation of Release that takes a forced version.
        
        // For now, let's implement it here or refactor Release
        return FinalizePromotion(info, dryRun);
    }

    private string FinalizePromotion(PrereleaseInfo info, bool dryRun)
    {
        // This is basically Release() but with a specific version
        var config = LoadConfig();
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        string changelogPath = Path.Combine(_basePath, config.ChangelogPath);

        GetStatus(out int fragmentCount, out ChangeSet mergedChangeSet, out _, out _);

        // Even if we promote, we still need fragments?
        // Yes, fragments are what we are releasing.
        if (fragmentCount == 0)
        {
            throw new InvalidOperationException("No unreleased fragments found to promote.");
        }

        string changelogContent = File.Exists(changelogPath)
            ? File.ReadAllText(changelogPath, Encoding.UTF8)
            : "# Changelog..."; // Simplified

        var changeLog = new ChangeLog(changelogContent);
        // We need a way to force the version in ChangeLog.Release
        // Let's add that.
        
        // For now, assume info.BaseVersion IS what ComputeVersion would give.
        // The spec says "without recalculating", so we should probably use info.BaseVersion.
        
        string resultVersion = Release(DateTime.Today, dryRun, info.BaseVersion);
        
        // Clean up prerelease info
        string branchSlug = SanitizeBranchName(info.Branch);
        string prereleaseDir = Path.Combine(_basePath, config.PrereleasesDir, branchSlug);
        if (Directory.Exists(prereleaseDir)) Directory.Delete(prereleaseDir, true);

        return resultVersion;
    }

    public class PrereleaseInfo
    {
        public string Version { get; set; } = "";
        public string BaseVersion { get; set; } = "";
        public int Counter { get; set; }
        public string Branch { get; set; } = "";
        public DateTime Timestamp { get; set; }
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

    public string GetCurrentBranch()
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --abbrev-ref HEAD",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _basePath
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null) return "main";
            
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                return output;
            }
        }
        catch
        {
            // Fallback if git is not available or not a repository
        }

        return "main";
    }

    public string SanitizeBranchName(string branchName)
    {
        var config = LoadConfig();
        if (!config.PreRelease.SanitizeBranchName) return branchName;

        // Replace / and other non-alphanumeric with hyphens
        string sanitized = Regex.Replace(branchName, @"[^a-zA-Z0-9]", "-");
        
        // Remove duplicate hyphens
        sanitized = Regex.Replace(sanitized, @"-+", "-").Trim('-');

        if (sanitized.Length > config.PreRelease.MaxIdentifierLength)
        {
            sanitized = sanitized.Substring(0, config.PreRelease.MaxIdentifierLength).Trim('-');
        }

        return sanitized.ToLowerInvariant();
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
