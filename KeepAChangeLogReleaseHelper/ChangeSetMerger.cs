namespace KeepAChangeLogReleaseHelper;

public class ChangeSetMerger
{
    public static ChangeSet Merge(string[] changeSets)
    {
        return Merge(changeSets
            .Select(changeSet => new ChangelogParser().Parse(changeSet)));
    }

    private static ChangeSet Merge(IEnumerable<ChangeSet> changeSets)
    {
        ChangeSet aggregate = changeSets
            .Aggregate(new ChangeSet(), (a, b) => a.Merge(b));
        return aggregate;
    }
}