using System.CommandLine;
using System.CommandLine.Parsing;

namespace ChangeSharp.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("ChangeSharp - Keep a Changelog. Derive the version.");

        // init command
        var initCommand = new Command("init", "Initialize ChangeSharp configuration and directory structure.");
        initCommand.SetAction(_ =>
        {
            try
            {
                var manager = new WorkspaceManager();
                manager.Initialize();
                Console.WriteLine("ChangeSharp workspace initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });
        rootCommand.Add(initCommand);

        // new command
        var messageArgument = new Argument<string>("message")
        {
            Description = "Description of the changes."
        };
        
        var addedOption     = new Option<bool>("--added",      "Mark change as Added.");
        var changedOption   = new Option<bool>("--changed",    "Mark change as Changed.");
        var fixedOption     = new Option<bool>("--fixed",      "Mark change as Fixed.");
        var removedOption   = new Option<bool>("--removed",    "Mark change as Removed.");
        var deprecatedOption= new Option<bool>("--deprecated", "Mark change as Deprecated.");
        var securityOption  = new Option<bool>("--security",   "Mark change as Security.");
        var breakingOption  = new Option<bool>("--breaking",   "Mark change as Breaking Changes.");

        var newCommand = new Command("new", "Create a new unreleased changelog fragment.")
        {
            messageArgument,
            addedOption,
            changedOption,
            fixedOption,
            removedOption,
            deprecatedOption,
            securityOption,
            breakingOption,
        };

        newCommand.SetAction(parseResult =>
        {
            string message    = parseResult.GetValue(messageArgument)!;
            bool added        = parseResult.GetValue(addedOption);
            bool changed      = parseResult.GetValue(changedOption);
            bool fixedOpt     = parseResult.GetValue(fixedOption);
            bool removed      = parseResult.GetValue(removedOption);
            bool deprecated   = parseResult.GetValue(deprecatedOption);
            bool security     = parseResult.GetValue(securityOption);
            bool breaking     = parseResult.GetValue(breakingOption);

            string category = breaking    ? "Breaking Changes"
                            : removed     ? "Removed"
                            : changed     ? "Changed"
                            : deprecated  ? "Deprecated"
                            : fixedOpt    ? "Fixed"
                            : security    ? "Security"
                                          : "Added"; // default / --added

            try
            {
                var manager = new WorkspaceManager();
                string filePath = manager.CreateFragment(message, category);
                Console.WriteLine($"Created fragment: {Path.GetFileName(filePath)} under category '{category}'");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });
        rootCommand.Add(newCommand);

        // status command
        var statusCommand = new Command("status", "Show the status of unreleased fragments and computed version bump.");
        statusCommand.SetAction(_ =>
        {
            try
            {
                var manager = new WorkspaceManager();
                manager.GetStatus(out int count, out ChangeSet merged, out string current, out string next);

                Console.WriteLine($"Unreleased fragments found: {count}");
                if (count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Aggregated Changes:");
                    Console.Write(merged.ToChangelogString());
                    Console.WriteLine();
                    Console.WriteLine($"Computed Version Bump: {current} -> {next}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });
        rootCommand.Add(statusCommand);

        // release command
        var dryRunOption = new Option<bool>("--dry-run", "Display what would happen without making any changes.");
        var releaseCommand = new Command("release", "Aggregate fragments, bump version, update CHANGELOG.md, and clean up.")
        {
            dryRunOption
        };
        releaseCommand.SetAction(parseResult =>
        {
            bool dryRun = parseResult.GetValue(dryRunOption);
            try
            {
                var manager = new WorkspaceManager();
                if (dryRun)
                {
                    manager.GetStatus(out int count, out ChangeSet merged, out string current, out string next);
                    
                    if (count == 0)
                    {
                        Console.WriteLine("No unreleased fragments found. Nothing to release.");
                        return;
                    }

                    Console.WriteLine("[Dry Run] Release would perform the following actions:");
                    Console.WriteLine($"- Update CHANGELOG.md with a new version section: [{next}]");
                    Console.WriteLine($"- Add the following changes to CHANGELOG.md:");
                    Console.WriteLine(merged.ToChangelogString());
                    Console.WriteLine($"- Delete {count} fragment(s) from the unreleased directory.");
                    
                    var targets = manager.GetEffectiveVersionTargets().ToList();
                    if (targets.Any())
                    {
                        Console.WriteLine($"- Propagate version {next} to the following files:");
                        foreach (var target in targets)
                        {
                            Console.WriteLine($"  * {target}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("- No version propagation targets configured.");
                    }
                    Console.WriteLine();
                    Console.WriteLine("[Dry Run] No files were actually modified.");
                }
                else
                {
                    string nextVersion = manager.Release(DateTime.Today, dryRun);
                    Console.WriteLine($"Release successful! New version: {nextVersion}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });
        rootCommand.Add(releaseCommand);
        
        // prerelease command
        var branchOption  = new Option<string>("--branch", "Specific branch name to use for pre-release.");
        var listOption    = new Option<bool>("--list", "List all active pre-releases.");
        var promoteOption = new Option<bool>("--promote", "Promote the latest pre-release to a final release.");
        var channelOption = new Option<string>("--channel", "Optional release channel (e.g. alpha, beta, rc).");
        
        var prereleaseCommand = new Command("prerelease", "Handle pre-release versions based on branches.")
        {
            branchOption,
            listOption,
            promoteOption,
            channelOption,
            dryRunOption
        };

        prereleaseCommand.SetAction(parseResult =>
        {
            string? branch = parseResult.GetValue(branchOption);
            bool list      = parseResult.GetValue(listOption);
            bool promote   = parseResult.GetValue(promoteOption);
            string? channel = parseResult.GetValue(channelOption);
            bool dryRun    = parseResult.GetValue(dryRunOption);

            try
            {
                var manager = new WorkspaceManager();
                if (list)
                {
                    var listPrereleases = manager.ListPrereleases();
                    if (!listPrereleases.Any())
                    {
                        Console.WriteLine("No active pre-releases found.");
                    }
                    else
                    {
                        Console.WriteLine("Active pre-releases:");
                        foreach (var info in listPrereleases)
                        {
                            Console.WriteLine($"- {info.Version} (Branch: {info.Branch}, Date: {info.Timestamp:yyyy-MM-dd HH:mm:ss})");
                        }
                    }
                }
                else if (promote)
                {
                    string finalVersion = manager.PromotePrerelease(branch, dryRun);
                    if (dryRun)
                    {
                        Console.WriteLine($"[Dry Run] Would promote to version: {finalVersion}");
                    }
                    else
                    {
                        Console.WriteLine($"Promotion successful! New version: {finalVersion}");
                    }
                }
                else
                {
                    string prereleaseVersion = manager.CreatePrerelease(branch, channel, dryRun);
                    if (dryRun)
                    {
                        Console.WriteLine($"[Dry Run] Would create pre-release: {prereleaseVersion}");
                    }
                    else
                    {
                        Console.WriteLine($"Pre-release created successfully: {prereleaseVersion}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });
        rootCommand.Add(prereleaseCommand);

        ParseResult parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }
}