namespace KeepAChangeLogReleaseHelper;

internal class ChangelogParser
{
    public ChangelogParser()
    {
    }

    public ChangeSet Parse(string changeset)
    {
        // Split changeset into lines
        IEnumerable<string> lines = changeset.Split('\n').Select(line => line.Trim());

        List<string> changed = new();
        List<string> added = new();
        List<string> removed = new();
        List<string> deprecated = new();
        List<string> @fixed = new();
        List<string> security = new();
        bool hasMajor = false;
        bool hasMinor = false;
        bool hasPatch = false;
        bool changedStarted = false;
        bool addedStarted = false;
        bool removedStarted = false;
        bool deprecatedStarted = false;
        bool fixedStarted = false;
        bool securityStarted = false;
        foreach (string line in lines)
        {
            if (line.StartsWith("## Changed", StringComparison.OrdinalIgnoreCase))
            {
                changedStarted = true;
                addedStarted = false;
                removedStarted = false;
                deprecatedStarted = false;
            }
            else if (line.StartsWith("## Added", StringComparison.OrdinalIgnoreCase))
            {
                changedStarted = false;
                addedStarted = true;
                removedStarted = false;
                deprecatedStarted = false;
            }
            else if (line.StartsWith("## Removed", StringComparison.OrdinalIgnoreCase))
            {
                changedStarted = false;
                addedStarted = false;
                removedStarted = true;
                deprecatedStarted = false;
            }
            else if (line.StartsWith("## Deprecated", StringComparison.OrdinalIgnoreCase))
            {
                changedStarted = false;
                addedStarted = false;
                removedStarted = false;
                deprecatedStarted = true;
            }
            else if (line.StartsWith("## Fixed", StringComparison.OrdinalIgnoreCase))
            {
                changedStarted = false;
                addedStarted = false;
                removedStarted = false;
                deprecatedStarted = false;
                fixedStarted = true;
                securityStarted = false;
            }
            else if (line.StartsWith("## Security", StringComparison.OrdinalIgnoreCase))
            {
                changedStarted = false;
                addedStarted = false;
                removedStarted = false;
                deprecatedStarted = false;
                fixedStarted = false;
                securityStarted = true;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (changedStarted)
                    {
                        changed.Add(line);
                        hasMajor = true;
                    }
                    else if (removedStarted)
                    {
                        removed.Add(line);
                        hasMajor = true;
                    }
                    else if (addedStarted)
                    {
                        added.Add(line);
                        hasMinor = true;
                    }
                    else if (deprecatedStarted)
                    {
                        deprecated.Add(line);
                        hasMinor = true;
                    }
                    else if (fixedStarted)
                    {
                        @fixed.Add(line);
                        hasPatch = true;
                    }
                    else if (securityStarted)
                    {
                        security.Add(line);
                        hasPatch = true;
                    }
                }
            }
        }

        return new ChangeSet()
        {
            Changed =  changed,
            Removed = removed,
            Added = added,
            Deprecated = deprecated,
            Fixed = @fixed,
            Security = security
        };
    }
}