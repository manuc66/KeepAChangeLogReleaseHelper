namespace KeepAChangeLogReleaseHelper;

public class ChangelogParser
{
    public ChangelogParser()
    {
    }

    internal ReleaseChangeLog Parse(string changeset)
    {
        // Split changeset into lines
        IEnumerable<string> lines = changeset.Split('\n').Select(line => line.Trim());

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
                    if (changedStarted || removedStarted)
                    {
                        hasMajor = true;
                    }

                    if (addedStarted || deprecatedStarted)
                    {
                        hasMinor = true;
                    }

                    if (fixedStarted || securityStarted)
                    {
                        hasPatch = true;
                    }
                }
            }
        }

        return new ReleaseChangeLog()
        {
            HasMajor = hasMajor,
            HasMinor = hasMinor,
            HasPatch = hasPatch,
        };
    }
}