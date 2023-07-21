namespace KeepAChangeLogReleaseHelper.Tests;

public class ChangeSetMergerTests
{
    [Test]
    public void ItComputeAMajorFromChanged()
    {
        string[] changesets =
        {
            @"
        ## Changed
        - Improved existing feature 1.

        ",
            @" ## Fixed
        - Bug fix 1.
        - Bug fix 2.
        ",
            @"
        ## Added
        - New feature 1.
        - New feature 2."
        };

        string mergedResult = ChangeSetMerger.Merge(changesets).ToString();

        Assert.That(mergedResult, Is.EqualTo(
            @"## Changed
- Improved existing feature 1.

## Added
- New feature 1.
- New feature 2.

## Fixed
- Bug fix 1.
- Bug fix 2.
"));
    }
}