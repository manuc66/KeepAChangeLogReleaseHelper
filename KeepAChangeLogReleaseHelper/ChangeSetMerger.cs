namespace KeepAChangeLogReleaseHelper;

public class ChangeSetMerger
{
    public static string Merge(string[] changeSets)
    {
        return changeSets
            .Select(changeSet => new ChangelogParser().Parse(changeSet))
            .Aggregate(new ChangeSet(), (a, b) => a.Merge(b))
            .ToString();
    }
}