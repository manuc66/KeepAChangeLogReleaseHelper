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
        var releaseCommand = new Command("release", "Aggregate fragments, bump version, update CHANGELOG.md, and clean up.");
        releaseCommand.SetAction(_ =>
        {
            try
            {
                var manager = new WorkspaceManager();
                string nextVersion = manager.Release(DateTime.Today);
                Console.WriteLine($"Release successful! New version: {nextVersion}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });
        rootCommand.Add(releaseCommand);

        ParseResult parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }
}