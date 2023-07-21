namespace KeepAChangeLogReleaseHelper.Tests;

public class ChangeLogTests
{
    [Test]
    public void ItCanUpdateTheUnreleasedSection()
    {
        var output = new ChangeLog(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.1] - 2014-05-31

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.
- CNAME file to enable GitHub Pages custom domain.
- README now contains answers to common questions about CHANGELOGs.
- Good examples and basic guidelines, including proper date formatting.
- Counter-examples: ""What makes unicorns cry?"".
").UpdateUnReleased(@"
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
        ).ToString();

        Assert.That(output, Is.EqualTo(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Improved existing feature 1.

### Added
- New feature 1.
- New feature 2.

### Fixed
- Bug fix 1.
- Bug fix 2.

## [0.0.1] - 2014-05-31

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.
- CNAME file to enable GitHub Pages custom domain.
- README now contains answers to common questions about CHANGELOGs.
- Good examples and basic guidelines, including proper date formatting.
- Counter-examples: ""What makes unicorns cry?"".
"));
    }

    [Test]
    public void ItCanUpdateTheUnreleasedSectionByReplacingExistingContent()
    {
        var output = new ChangeLog(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Improved existing feature qsdqsnldnk.

### Added
- New feature fdgdfg.

### Secirity
- Address CVS-2131314

## [0.0.1] - 2014-05-31

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.
- CNAME file to enable GitHub Pages custom domain.
- README now contains answers to common questions about CHANGELOGs.
- Good examples and basic guidelines, including proper date formatting.
- Counter-examples: ""What makes unicorns cry?"".
").UpdateUnReleased(@"
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
        ).ToString();

        Assert.That(output, Is.EqualTo(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Improved existing feature 1.

### Added
- New feature 1.
- New feature 2.

### Fixed
- Bug fix 1.
- Bug fix 2.

## [0.0.1] - 2014-05-31

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.
- CNAME file to enable GitHub Pages custom domain.
- README now contains answers to common questions about CHANGELOGs.
- Good examples and basic guidelines, including proper date formatting.
- Counter-examples: ""What makes unicorns cry?"".
"));
    }
    [Test]
    public void ItCanGetLastReleaseWhenUnlreseaseIsPresent()
    {
        ChangeLog changeLog = new ChangeLog(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Improved existing feature qsdqsnldnk.

### Added
- New feature fdgdfg.

### Secirity
- Address CVS-2131314

## [0.1.0] - 2014-05-31

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.
- CNAME file to enable GitHub Pages custom domain.
- README now contains answers to common questions about CHANGELOGs.
- Good examples and basic guidelines, including proper date formatting.
- Counter-examples: ""What makes unicorns cry?"".
");
        
        Assert.That(changeLog.LastVersion, Is.EqualTo("0.1.0"));
    }
    [Test]
    public void ItCanGetLastReleaseWhenUnlreseaseIsNotThere()
    {
        ChangeLog changeLog = new ChangeLog(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2014-05-31

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.
- CNAME file to enable GitHub Pages custom domain.
- README now contains answers to common questions about CHANGELOGs.
- Good examples and basic guidelines, including proper date formatting.
- Counter-examples: ""What makes unicorns cry?"".
");
        
        Assert.That(changeLog.LastVersion, Is.EqualTo("0.1.0"));
    }
    [Test]
    public void ItCanGetLastReleaseWhenUNoVersion()
    {
        ChangeLog changeLog = new ChangeLog(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).");
        
        Assert.That(changeLog.LastVersion, Is.EqualTo("0.0.0"));
    }


    [Test]
    public void ItCanProduceARelease()
    {
        ChangeLog changeLog = new ChangeLog(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Improved existing feature qsdqsnldnk.

### Added
- New feature fdgdfg.

### Secirity
- Address CVS-2131314

## [0.1.0] - 2014-05-31

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.
- CNAME file to enable GitHub Pages custom domain.
- README now contains answers to common questions about CHANGELOGs.
- Good examples and basic guidelines, including proper date formatting.
- Counter-examples: ""What makes unicorns cry?"".
").Release(new DateTime(2023, 7, 9, 12, 5, 2, 3, 11),@"
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
        );
        
        Assert.That(changeLog.LastVersion, Is.EqualTo("1.0.0"));
        Assert.That(changeLog.ToString(), Is.EqualTo(@"# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2023-07-09

### Changed
- Improved existing feature 1.

### Added
- New feature 1.
- New feature 2.

### Fixed
- Bug fix 1.
- Bug fix 2.

## [0.1.0] - 2014-05-31

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a
  standardized open source project CHANGELOG.
- CNAME file to enable GitHub Pages custom domain.
- README now contains answers to common questions about CHANGELOGs.
- Good examples and basic guidelines, including proper date formatting.
- Counter-examples: ""What makes unicorns cry?"".
"));
    }
}