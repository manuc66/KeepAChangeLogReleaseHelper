namespace KeepAChangeLogReleaseHelper;

public class ChangeLog
{
    private readonly string _content;

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
}