using System.CommandLine;
using System.CommandLine.Parsing;

namespace ChangeSharp.Cli;

class Program
{
    private const int ExitCodeSuccess = 0;
    private const int ExitCodeGenericError = 1;
    private const int ExitCodeNoChanges = 2;
    private const int ExitCodeValidationError = 3;
    private const int ExitCodeConflict = 4;

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
                bool configExists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "changesharp.json"));
                var targets = manager.Initialize();
                
                if (configExists)
                {
                    if (targets.Any())
                    {
                        Console.WriteLine("ChangeSharp workspace updated with new components.");
                        Console.WriteLine("Added version targets:");
                        foreach (var target in targets)
                        {
                            Console.WriteLine($"  - {target.Path} ({target.Type})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ChangeSharp workspace is already up to date. No new components discovered.");
                    }
                }
                else
                {
                    Console.WriteLine("ChangeSharp workspace initialized successfully.");
                    if (targets.Any())
                    {
                        Console.WriteLine("Auto-discovered version targets:");
                        foreach (var target in targets)
                        {
                            Console.WriteLine($"  - {target.Path} ({target.Type})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No version targets were auto-discovered. You can add them manually to changesharp.json.");
                    }
                }
                return ExitCodeSuccess;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return ExitCodeGenericError;
            }
        });
        rootCommand.Add(initCommand);

        // new command
        var messageArgument = new Argument<string>("message")
        {
            Description = "Description of the changes.",
            Arity = ArgumentArity.ZeroOrOne
        };
        
        var addedOption      = new Option<bool>("--added")      { Description = "Mark change as Added." };
        var changedOption    = new Option<bool>("--changed")    { Description = "Mark change as Changed." };
        var fixedOption      = new Option<bool>("--fixed")      { Description = "Mark change as Fixed." };
        var removedOption    = new Option<bool>("--removed")    { Description = "Mark change as Removed." };
        var deprecatedOption = new Option<bool>("--deprecated") { Description = "Mark change as Deprecated." };
        var securityOption   = new Option<bool>("--security")   { Description = "Mark change as Security." };
        var breakingOption   = new Option<bool>("--breaking")   { Description = "Mark change as Breaking Changes." };

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
            string? message = parseResult.GetValue(messageArgument);
            if (string.IsNullOrWhiteSpace(message))
            {
                message = PromptForMessage();
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.Error.WriteLine("Error: Description is required.");
                return ExitCodeValidationError;
            }

            string category;
            bool added = parseResult.GetValue(addedOption);
            bool changed = parseResult.GetValue(changedOption);
            bool fixedOpt = parseResult.GetValue(fixedOption);
            bool removed = parseResult.GetValue(removedOption);
            bool deprecated = parseResult.GetValue(deprecatedOption);
            bool security = parseResult.GetValue(securityOption);
            bool breaking = parseResult.GetValue(breakingOption);

            bool anyCategoryOptionProvided = added || changed || fixedOpt || removed || deprecated || security || breaking;

            if (anyCategoryOptionProvided)
            {
                category = breaking ? "Breaking Changes"
                                 : removed ? "Removed"
                                 : changed ? "Changed"
                                 : deprecated ? "Deprecated"
                                 : fixedOpt ? "Fixed"
                                 : security ? "Security"
                                              : "Added";
            }
            else
            {
                category = PromptForCategory();
            }

            try
            {
                var manager = new WorkspaceManager();
                string filePath = manager.CreateFragment(message, category);
                Console.WriteLine($"Created fragment: {Path.GetFileName(filePath)} under category '{category}'");
                return ExitCodeSuccess;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return ExitCodeGenericError;
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

                var newTargets = manager.DiscoverNewTargets();
                if (newTargets.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("Warning: New components discovered but not tracked in changesharp.json:");
                    foreach (var target in newTargets)
                    {
                        Console.WriteLine($"  - {target.Path} ({target.Type})");
                    }
                    Console.WriteLine("Run 'changesharp init' to add them to your configuration.");
                }

                return ExitCodeSuccess;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return ExitCodeGenericError;
            }
        });
        rootCommand.Add(statusCommand);

        // validate command
        var validateCommand = new Command("validate", "Validate unreleased fragments for correct format.");
        validateCommand.SetAction(_ =>
        {
            try
            {
                var manager = new WorkspaceManager();
                var results = manager.Validate();

                if (results.Count == 0)
                {
                    Console.WriteLine("No unreleased fragments found to validate.");
                    return ExitCodeSuccess;
                }

                int errorCount = 0;
                foreach (var result in results)
                {
                    if (result.IsValid)
                    {
                        Console.WriteLine($"✓ {result.FilePath}: Valid");
                    }
                    else
                    {
                        Console.WriteLine($"✗ {result.FilePath}: Invalid");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"  - {error}");
                        }
                        errorCount++;
                    }
                }

                if (errorCount > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"{errorCount} fragment(s) failed validation.");
                    return ExitCodeValidationError;
                }

                Console.WriteLine();
                Console.WriteLine("All fragments are valid.");
                return ExitCodeSuccess;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return ExitCodeGenericError;
            }
        });
        rootCommand.Add(validateCommand);

        // release command
        var dryRunOption = new Option<bool>("--dry-run") { Description = "Display what would happen without making any changes." };
        var allowEmptyOption = new Option<bool>("--allow-empty") { Description = "Exit with success even if no unreleased fragments are found." };
        var releaseCommand = new Command("release", "Aggregate fragments, bump version, update CHANGELOG.md, and clean up.")
        {
            dryRunOption,
            allowEmptyOption
        };
        releaseCommand.SetAction(parseResult =>
        {
            bool dryRun = parseResult.GetValue(dryRunOption);
            bool allowEmpty = parseResult.GetValue(allowEmptyOption);
            try
            {
                var manager = new WorkspaceManager();
                manager.GetStatus(out int count, out ChangeSet merged, out string current, out string next);
                
                if (count == 0)
                {
                    if (allowEmpty)
                    {
                        Console.WriteLine("No unreleased fragments found. --allow-empty specified, exiting with success.");
                        return ExitCodeSuccess;
                    }
                    Console.Error.WriteLine("Error: No unreleased fragments found. Nothing to release.");
                    return ExitCodeNoChanges;
                }

                if (dryRun)
                {
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
                return ExitCodeSuccess;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Conflict"))
            {
                Console.Error.WriteLine($"Conflict Error: {ex.Message}");
                return ExitCodeConflict;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return ExitCodeGenericError;
            }
        });
        rootCommand.Add(releaseCommand);
        
        // prerelease command
        var branchOption  = new Option<string>("--branch") { Description = "Specific branch name to use for pre-release." };
        var listOption    = new Option<bool>("--list") { Description = "List all active pre-releases." };
        var promoteOption = new Option<bool>("--promote") { Description = "Promote the latest pre-release to a final release." };
        var channelOption = new Option<string>("--channel") { Description = "Optional release channel (e.g. alpha, beta, rc)." };
        
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
                return ExitCodeSuccess;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Conflict"))
            {
                Console.Error.WriteLine($"Conflict Error: {ex.Message}");
                return ExitCodeConflict;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return ExitCodeGenericError;
            }
        });
        rootCommand.Add(prereleaseCommand);

        ParseResult parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    private static string? PromptForMessage()
    {
        Console.Write("Enter a description for the change: ");
        return Console.ReadLine();
    }

    private static string PromptForCategory()
    {
        Console.WriteLine("Select a category for the change:");
        Console.WriteLine("1. Added (New feature)");
        Console.WriteLine("2. Changed (Modification of existing feature)");
        Console.WriteLine("3. Fixed (Bug fix)");
        Console.WriteLine("4. Removed (Removal of a feature)");
        Console.WriteLine("5. Deprecated (Future removal warning)");
        Console.WriteLine("6. Security (Security improvement)");
        Console.WriteLine("7. Breaking Changes (Backward incompatible change)");
        Console.Write("Selection (1-7, default 1): ");

        string? input = Console.ReadLine();
        return input switch
        {
            "2" => "Changed",
            "3" => "Fixed",
            "4" => "Removed",
            "5" => "Deprecated",
            "6" => "Security",
            "7" => "Breaking Changes",
            _ => "Added"
        };
    }
}