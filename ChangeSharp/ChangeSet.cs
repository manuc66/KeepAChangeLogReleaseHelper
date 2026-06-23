using System.Text;

namespace ChangeSharp;

public class ChangeSet
{
    public Dictionary<string, List<string>> Sections { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> Breaking => GetSection("Breaking Changes");
    public List<string> Changed => GetSection("Changed");
    public List<string> Removed => GetSection("Removed");
    public List<string> Added => GetSection("Added");
    public List<string> Deprecated => GetSection("Deprecated");
    public List<string> Fixed => GetSection("Fixed");
    public List<string> Security => GetSection("Security");

    public List<string> GetSection(string name)
    {
        if (!Sections.TryGetValue(name, out var list))
        {
            list = new List<string>();
            Sections[name] = list;
        }
        return list;
    }

    public ChangeSet Merge(ChangeSet other)
    {
        var result = new ChangeSet();
        foreach (var pair in Sections)
        {
            result.GetSection(pair.Key).AddRange(pair.Value);
        }
        foreach (var pair in other.Sections)
        {
            var section = result.GetSection(pair.Key);
            foreach (var item in pair.Value)
            {
                if (!section.Contains(item)) section.Add(item);
            }
        }
        return result;
    }

    public bool IsEmpty()
    {
        return Sections.Values.All(v => v.Count == 0);
    }

    public override string ToString()
    {
        return ToMarkdownString("##");
    }

    public string ToChangelogString()
    {
        return ToMarkdownString("###");
    }

    private string ToMarkdownString(string level)
    {
        StringBuilder sb = new();
        var preferredOrder = new[] { "Breaking Changes", "Changed", "Removed", "Added", "Deprecated", "Fixed", "Security" };
        var handled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in preferredOrder)
        {
            if (Sections.TryGetValue(name, out var items) && items.Count > 0)
            {
                AddSection(sb, name, items, level);
                handled.Add(name);
            }
        }

        foreach (var pair in Sections)
        {
            if (!handled.Contains(pair.Key) && pair.Value.Count > 0)
            {
                AddSection(sb, pair.Key, pair.Value, level);
            }
        }

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