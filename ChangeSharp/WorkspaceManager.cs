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

    public List<VersionTargetConfig> Initialize()
    {
        // 1. Create default configuration
        bool isNewConfig = !File.Exists(ConfigFilePath);
        var config = isNewConfig ? new ChangeSharpConfig() : LoadConfig();

        // Auto-discovery of version targets
        var discoveredTargets = DiscoverVersionTargets();
        var newTargets = new List<VersionTargetConfig>();

        if (isNewConfig)
        {
            config.VersionTargets.AddRange(discoveredTargets);
            newTargets.AddRange(discoveredTargets);
        }
        else
        {
            var existingPaths = new HashSet<string>(config.VersionTargets.Select(t => t.Path), StringComparer.OrdinalIgnoreCase);
            foreach (var discovered in discoveredTargets)
            {
                if (!existingPaths.Contains(discovered.Path))
                {
                    config.VersionTargets.Add(discovered);
                    newTargets.Add(discovered);
                }
            }
        }

        if (isNewConfig || newTargets.Any())
        {
            // Sort targets by path to ensure deterministic JSON and minimize merge conflicts
            config.VersionTargets = config.VersionTargets
                .OrderBy(t => t.Path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigFilePath, json, Encoding.UTF8);
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

        // 2c. Create releasing directory
        string releasingPath = Path.Combine(_basePath, config.ReleasingDir);
        if (!Directory.Exists(releasingPath))
        {
            Directory.CreateDirectory(releasingPath);
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

        return newTargets;
    }

    public List<VersionTargetConfig> DiscoverNewTargets()
    {
        if (!File.Exists(ConfigFilePath)) return DiscoverVersionTargets();

        var config = LoadConfig();
        var allDiscovered = DiscoverVersionTargets();

        var existingPaths = new HashSet<string>(config.VersionTargets.Select(t => t.Path), StringComparer.OrdinalIgnoreCase);

        return allDiscovered.Where(t => !existingPaths.Contains(t.Path)).ToList();
    }

    public List<VersionTargetConfig> DiscoverVersionTargets()
    {
        var targets = new List<VersionTargetConfig>();
        var rootDir = new DirectoryInfo(_basePath);

        // Patterns to look for
        var msbuildPatterns = new[] { "*.csproj", "Directory.Build.props" };
        var jsonPatterns = new[] { "package.json" };

        // Directories to skip
        var skipDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj", ".git", "node_modules", ".changesharp" };

        DiscoverRecursive(rootDir, targets, msbuildPatterns, jsonPatterns, skipDirs, 0, 3); // max depth 3

        return targets;
    }

    private void DiscoverRecursive(DirectoryInfo dir, List<VersionTargetConfig> targets, string[] msbuildPatterns, string[] jsonPatterns, HashSet<string> skipDirs, int currentDepth, int maxDepth)
    {
        if (currentDepth > maxDepth) return;
        if (skipDirs.Contains(dir.Name)) return;

        foreach (var pattern in msbuildPatterns)
        {
            foreach (var file in dir.GetFiles(pattern))
            {
                targets.Add(new VersionTargetConfig
                {
                    Path = Path.GetRelativePath(_basePath, file.FullName),
                    Type = "msbuild"
                });
            }
        }

        foreach (var pattern in jsonPatterns)
        {
            foreach (var file in dir.GetFiles(pattern))
            {
                targets.Add(new VersionTargetConfig
                {
                    Path = Path.GetRelativePath(_basePath, file.FullName),
                    Type = "json",
                    JsonPath = "version"
                });
            }
        }

        foreach (var subDir in dir.GetDirectories())
        {
            DiscoverRecursive(subDir, targets, msbuildPatterns, jsonPatterns, skipDirs, currentDepth + 1, maxDepth);
        }
    }

    public List<ValidationResult> Validate()
    {
        var config = LoadConfig();
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        if (!Directory.Exists(unreleasedPath)) return new List<ValidationResult>();

        var files = Directory.GetFiles(unreleasedPath, "*.md");
        var results = new List<ValidationResult>();
        var parser = new ChangelogParser();

        foreach (var file in files)
        {
            var result = new ValidationResult { FilePath = Path.GetRelativePath(_basePath, file) };
            string content = File.ReadAllText(file, Encoding.UTF8);

            // Simple validation using the parser
            var changeSet = parser.Parse(content);

            if (changeSet.IsEmpty())
            {
                result.Errors.Add("Fragment is empty.");
            }
            else if (changeSet.Sections.Keys.All(k => !config.SemverPolicy.Mappings.ContainsKey(k)))
            {
                string validCats = string.Join(", ", config.SemverPolicy.Mappings.Keys);
                result.Errors.Add($"Fragment has no recognized categories. Recognized: {validCats}");
            }

            // More specific validation using Markdig directly
            var doc = Markdig.Markdown.Parse(content);
            if (doc.Count == 0)
            {
                result.Errors.Add("Markdown file is empty.");
            }
            else
            {
                var firstBlock = doc.FirstOrDefault();
                if (firstBlock is not Markdig.Syntax.HeadingBlock hb || hb.Level != 3)
                {
                    result.Errors.Add("Fragment must start with a level 3 heading (### Category).");
                }
            }

            result.IsValid = result.Errors.Count == 0;
            results.Add(result);
        }

        return results;
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
        
        string filename;
        if (config.FragmentNaming.IncludeBranchName)
        {
            string branch = GetGitBranchName() ?? "";
            string branchSlug = string.IsNullOrWhiteSpace(branch) ? "" : Slugify(branch) + "-";
            filename = $"{timestamp}-{branchSlug}{slug}.md";
        }
        else
        {
            filename = $"{timestamp}-{slug}.md";
        }

        string filePath = Path.Combine(unreleasedPath, filename);

        // Normalize category naming (e.g. "Breaking Changes" vs "Added")
        string formattedCategory = NormalizeCategory(category);

        string content = $"### {formattedCategory}{Environment.NewLine}- {message}{Environment.NewLine}";
        File.WriteAllText(filePath, content, Encoding.UTF8);

        return filePath;
    }

    private string? GetGitBranchName()
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
            if (process == null) return null;
            
            string branch = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            return process.ExitCode == 0 ? branch : null;
        }
        catch
        {
            return null;
        }
    }

    public void GetStatus(
        out int fragmentCount,
        out ChangeSet mergedChangeSet,
        out string currentVersion,
        out string nextVersion)
    {
        var config = LoadConfig();
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        string releasingPath = Path.Combine(_basePath, config.ReleasingDir);
        
        var unreleasedFragments = Directory.Exists(unreleasedPath)
            ? Directory.GetFiles(unreleasedPath, "*.md")
            : Array.Empty<string>();

        var releasingFragments = Directory.Exists(releasingPath)
            ? Directory.GetFiles(releasingPath, "*.md")
            : Array.Empty<string>();

        var allFragments = unreleasedFragments.Concat(releasingFragments).ToArray();
        fragmentCount = allFragments.Length;

        var parser = new ChangelogParser();
        var changeSets = new List<ChangeSet>();

        foreach (var file in allFragments)
        {
            string fileContent = File.ReadAllText(file, Encoding.UTF8);
            changeSets.Add(parser.Parse(fileContent));
        }

        mergedChangeSet = changeSets.Count > 0
            ? changeSets.Aggregate(new ChangeSet(), (a, b) => a.Merge(b))
            : new ChangeSet();

        currentVersion = GetCurrentVersion(config);
        nextVersion = NextVersionComputer.ComputeVersion(currentVersion, mergedChangeSet, config.SemverPolicy);
    }

    public string Release(DateTime releaseDate, bool dryRun = false, string? forcedVersion = null)
    {
        var config = LoadConfig();
        string unreleasedPath = Path.Combine(_basePath, config.UnreleasedDir);
        string releasingPath = Path.Combine(_basePath, config.ReleasingDir);
        string changelogPath = Path.Combine(_basePath, config.ChangelogPath);

        if (dryRun)
        {
            GetStatus(out int count, out ChangeSet merged, out string current, out string next);
            if (forcedVersion != null) next = forcedVersion;
            if (count == 0) throw new InvalidOperationException("No unreleased fragments found to release.");
            return next;
        }

        // 1. Transactional check & move
        string[] fragments;
        bool resumed = false;
        
        if (Directory.Exists(releasingPath) && Directory.GetFiles(releasingPath, "*.md").Length > 0)
        {
            fragments = Directory.GetFiles(releasingPath, "*.md");
            resumed = true;
        }
        else
        {
            var unreleasedFragments = Directory.Exists(unreleasedPath)
                ? Directory.GetFiles(unreleasedPath, "*.md")
                : Array.Empty<string>();

            if (unreleasedFragments.Length == 0)
            {
                throw new InvalidOperationException("No unreleased fragments found to release.");
            }

            if (!Directory.Exists(releasingPath)) Directory.CreateDirectory(releasingPath);
            foreach (var f in unreleasedFragments)
            {
                File.Move(f, Path.Combine(releasingPath, Path.GetFileName(f)));
            }
            fragments = Directory.GetFiles(releasingPath, "*.md");
        }

        // 2. Compute release data
        var parser = new ChangelogParser();
        var changeSets = fragments.Select(f => parser.Parse(File.ReadAllText(f, Encoding.UTF8))).ToList();
        var mergedChangeSet = changeSets.Aggregate(new ChangeSet(), (a, b) => a.Merge(b));
        
        string currentVersion = GetCurrentVersion(config);
        
        // 3. Update CHANGELOG.md
        string changelogContent = File.Exists(changelogPath) 
            ? File.ReadAllText(changelogPath, Encoding.UTF8) 
            : GetDefaultChangelog();
            
        var changeLog = new ChangeLog(changelogContent);
        string nextVersion;

        // Check if current version already contains these changes (resumed run)
        string? currentContent = changeLog.GetVersionContent(currentVersion);
        if (currentVersion != "0.0.0" && currentContent != null && currentContent == mergedChangeSet.ToChangelogString().Trim())
        {
            nextVersion = currentVersion;
            // Already updated in a previous interrupted run, skip update
        }
        else
        {
            nextVersion = forcedVersion ?? NextVersionComputer.ComputeVersion(currentVersion, mergedChangeSet, config.SemverPolicy);
            
            string? existingContent = changeLog.GetVersionContent(nextVersion);
            if (existingContent != null && existingContent == mergedChangeSet.ToChangelogString().Trim())
            {
                // This covers the case where nextVersion was already created but is not currentVersion 
                // (shouldn't happen with standard logic but good for robustness)
            }
            else if (existingContent != null)
            {
                throw new InvalidOperationException($"Conflict: Version {nextVersion} already exists in {config.ChangelogPath} but with different content. Manual intervention required.");
            }
            else
            {
                var updatedChangelog = changeLog.ReleaseWithVersion(releaseDate, nextVersion, mergedChangeSet.ToChangelogString());
                File.WriteAllText(changelogPath, updatedChangelog.ToString(), Encoding.UTF8);
            }
        }

        // 4. Cleanup processed fragments
        foreach (var file in fragments)
        {
            File.Delete(file);
        }

        // 5. Propagate version to targets
        PropagateVersion(config, nextVersion);

        return nextVersion;
    }

    private string GetCurrentVersion(ChangeSharpConfig config)
    {
        string changelogPath = Path.Combine(_basePath, config.ChangelogPath);
        if (File.Exists(changelogPath))
        {
            string changelogContent = File.ReadAllText(changelogPath, Encoding.UTF8);
            var changeLog = new ChangeLog(changelogContent);
            return changeLog.LastVersion;
        }
        return "0.0.0";
    }

    private string GetDefaultChangelog()
    {
        return @"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
";
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

        string nextPrerelease = NextVersionComputer.ComputePrereleaseVersion(current, merged, identifier, counter, config.SemverPolicy);

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

    private string NormalizeCategory(string category)
    {
        var config = LoadConfig();
        string cleaned = category.Trim();

        foreach (var mapping in config.SemverPolicy.Mappings)
        {
            if (mapping.Key.Equals(cleaned, StringComparison.OrdinalIgnoreCase))
            {
                return mapping.Key;
            }
        }

        // Special case for "breaking" -> "Breaking Changes"
        if (cleaned.Equals("breaking", StringComparison.OrdinalIgnoreCase) && config.SemverPolicy.Mappings.ContainsKey("Breaking Changes"))
        {
            return "Breaking Changes";
        }

        return cleaned;
    }
}
