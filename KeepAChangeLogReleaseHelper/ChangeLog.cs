namespace KeepAChangeLogReleaseHelper;

public class ChangeLog
{
    private readonly string _content;
    private string? _lastVersion = null;
    
    public string LastVersion
    {
        get {
            if (_lastVersion != null)
            {
                return _lastVersion;
            } 
            Parse(out List<string> _, out string lastRelease);

            return lastRelease;
        }
    }


    public ChangeLog(string content)
    {
        _content = content;
    }


    public ChangeLog UpdateUnReleased(params string[] changesets)
    {
        string unreleasedChanges = ChangeSetMerger.Merge(changesets).ToChangelogString();

        string[] lines = _content.Split(Environment.NewLine);

        List<string> beforeUnreleased = lines.TakeWhile(x => !x.StartsWith("## [Unreleased]")).ToList();

        IEnumerable<string> afterUnreleased = lines.Skip(beforeUnreleased.Count + 1)
            .SkipWhile(x => !x.StartsWith("## "));
        
        string newContent = string.Join(Environment.NewLine,
            beforeUnreleased.Concat(new[]
            {
                "## [Unreleased]" + Environment.NewLine, unreleasedChanges
            }).Concat(afterUnreleased)
        );

        return new ChangeLog(newContent);
    }

    public override string ToString()
    {
        return _content;
    }

    public ChangeLog Release(DateTime dateTime, params string[] changesets )
    {
        ChangeSet unreleasedChanges = ChangeSetMerger.Merge(changesets);

        List<string> beforeUnreleased = Parse(out List<string> afterUnreleased, out string lastRelease);

        string newVersion = NextVersionComputer.ComputeVersion(lastRelease, unreleasedChanges);

        string newContent = string.Join(Environment.NewLine,
            beforeUnreleased.Concat(new[]
            {
                $"## [{newVersion}] - {dateTime:yyyy-MM-dd}{Environment.NewLine}", unreleasedChanges.ToChangelogString()
            }).Concat(afterUnreleased)
        );

        ChangeLog changeLog = new(newContent)
        {
            _lastVersion = newVersion
        };
        return changeLog;
    }

    private List<string> Parse(out List<string> afterUnreleased, out string lastRelease)
    {
        string[] lines = _content.Split(Environment.NewLine);

        List<string> beforeUnreleased = lines.TakeWhile(x => !x.StartsWith("## [Unreleased]") && !x.StartsWith("## [")).ToList();

        if (lines.Length > beforeUnreleased.Count && lines[beforeUnreleased.Count].StartsWith("## [Unreleased]"))
        {
            afterUnreleased = lines.Skip(beforeUnreleased.Count + 1)
                .SkipWhile(x => !x.StartsWith("## ")).ToList(); 
        }
        else
        {
            afterUnreleased = lines.Skip(beforeUnreleased.Count)
                .SkipWhile(x => !x.StartsWith("## ")).ToList(); 
        }
        
        string? lastReleaseLine = afterUnreleased.Count > 1 ? afterUnreleased[0] : null;

        string? tempLastRelease = null;
        if (lastReleaseLine != null)
        {
            int indexOfEndVersion = lastReleaseLine.IndexOf("]", StringComparison.Ordinal) - 1;
            if (indexOfEndVersion > 4)
            {
                tempLastRelease = lastReleaseLine.Substring(4, indexOfEndVersion - 4 + 1);
            }
        }

        tempLastRelease ??= "0.0.0";

        lastRelease = tempLastRelease;

        return beforeUnreleased;
    }
}