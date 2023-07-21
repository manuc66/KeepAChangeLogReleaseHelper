using System.Text;

namespace KeepAChangeLogReleaseHelper;

public class ChangeSet
{
    public List<string> Changed { get; init; } = new();
    public List<string> Removed { get; init; } = new();
    public List<string> Added { get; init; } = new();
    public List<string> Deprecated { get; init; } = new();
    public List<string> Fixed { get; init; } = new();
    public List<string> Security { get; init; } = new();

    public ChangeSet Merge(ChangeSet other)
    {
        return new ChangeSet
        {
            Changed = Changed.Union(other.Changed).ToList(),
            Removed = Removed.Union(other.Removed).ToList(),
            Added = Added.Union(other.Added).ToList(),
            Deprecated = Deprecated.Union(other.Deprecated).ToList(),
            Fixed = Fixed.Union(other.Fixed).ToList(),
            Security = Security.Union(other.Security).ToList(),
        };
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        string level = "##";
        AddSection(sb, nameof(Changed), Changed, level);
        AddSection(sb, nameof(Removed), Removed, level);
        AddSection(sb, nameof(Added), Added, level);
        AddSection(sb, nameof(Deprecated), Deprecated, level);
        AddSection(sb, nameof(Fixed), Fixed, level);
        AddSection(sb, nameof(Security), Security, level);

        return sb.ToString();
    }

    public string ToChangelogString()
    {
        StringBuilder sb = new();

        string level = "###";
        AddSection(sb, nameof(Changed), Changed, level);
        AddSection(sb, nameof(Removed), Removed, level);
        AddSection(sb, nameof(Added), Added, level);
        AddSection(sb, nameof(Deprecated), Deprecated, level);
        AddSection(sb, nameof(Fixed), Fixed, level);
        AddSection(sb, nameof(Security), Security, level);

        return sb.ToString();
    }

    private void AddSection(StringBuilder sb, string name, List<string> items, string level)
    {
        if (items.Count <= 0) return;

        if (sb.Length > 0)
        {
            sb.AppendLine();
        }

        sb.AppendLine($"{level} {name}");
        items.ForEach(x => sb.AppendLine(x));
    }
}