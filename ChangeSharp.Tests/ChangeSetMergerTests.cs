namespace ChangeSharp.Tests;

public class ChangelogParserTests
{
    [Test]
    public void Parse_DeindentDoesNotCorruptNonHeadingHashLines()
    {
        // Lines with "##" or "# " after trimming should NOT be treated as headings
        // by Deindent — only "###" lines should be. Old code would treat any line
        // starting with "#" as a heading, trimming its indent and excluding it
        // from common deindentation.
        var fragment = @"  ### Changed
  ## Not a changelog section (just content)
  - Item under Changed
";
        var parser = new ChangelogParser();
        var result = parser.Parse(fragment);

        Assert.That(result.Sections.ContainsKey("Changed"), Is.True);
        var items = result.Sections["Changed"];
        // "## Not a changelog section" should be treated as content (a paragraph block)
        // since the parser only processes level-3 headings.
        // But Markdig interprets "## ..." as heading level 2, so it's skipped by the parser.
        // The important thing is: Deindent did not corrupt the line or its position.
        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("- Item under Changed"));
    }

    [Test]
    public void Parse_NoHeadings_ReturnsEmptyChangeSet()
    {
        var fragment = "Some plain text without any markdown headings.";
        var parser = new ChangelogParser();
        var result = parser.Parse(fragment);
        Assert.That(result.IsEmpty(), Is.True);
    }

    [Test]
    public void Parse_PreservesContentWithInlineHash()
    {
        var fragment = @"### Fixed
- Bug #42 is fixed
- Fixed issue #123
";
        var parser = new ChangelogParser();
        var result = parser.Parse(fragment);

        Assert.That(result.Sections.ContainsKey("Fixed"), Is.True);
        var items = result.Sections["Fixed"];
        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items[0], Does.Contain("Bug #42"));
        Assert.That(items[1], Does.Contain("issue #123"));
    }
}

public class ChangeSetMergerTests
{
    [Test]
    public void ItComputeAMajorFromChanged()
    {
        string[] changesets =
        {
            @"
        ### Changed
        - Improved existing feature 1.

        ",
            @" ### Fixed
        - Bug fix 1.
        - Bug fix 2.
        ",
            @"
        ### Added
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