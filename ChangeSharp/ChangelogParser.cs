using Markdig;
using Markdig.Syntax;

namespace ChangeSharp;

internal class ChangelogParser
{
    public ChangelogParser()
    {
    }

    public ChangeSet Parse(string changeset)
    {
        var result = new ChangeSet();
        if (string.IsNullOrWhiteSpace(changeset))
        {
            return result;
        }

        // Strip common leading whitespace to avoid standard Markdown parser treating 
        // indented blocks as code blocks.
        string deindented = Deindent(changeset);

        var document = Markdown.Parse(deindented);
        List<string>? currentList = null;

        foreach (var block in document)
        {
            if (block is HeadingBlock headingBlock)
            {
                // Extract heading text from the source line
                string headingText = deindented.Substring(headingBlock.Span.Start, headingBlock.Span.Length)
                    .TrimStart('#')
                    .Trim();

                currentList = result.GetSection(headingText);
            }
            else if (currentList != null)
            {
                if (block is ListBlock listBlock)
                {
                    foreach (var listItem in listBlock)
                    {
                        string itemText = deindented.Substring(listItem.Span.Start, listItem.Span.Length).Trim();
                        if (!string.IsNullOrWhiteSpace(itemText))
                        {
                            currentList.Add(itemText);
                        }
                    }
                }
                else
                {
                    string blockText = deindented.Substring(block.Span.Start, block.Span.Length).Trim();
                    if (!string.IsNullOrWhiteSpace(blockText))
                    {
                        currentList.Add(blockText);
                    }
                }
            }
        }

        return result;
    }

    private static string Deindent(string input)
    {
        var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        
        // Step 1: Identify headings and trim them, and find min indent of non-heading lines
        int minIndent = int.MaxValue;
        
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart(' ', '\t');
            if (trimmed.StartsWith("#"))
            {
                lines[i] = trimmed; // Trim headings completely
            }
            else if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                int indent = 0;
                while (indent < lines[i].Length && (lines[i][indent] == ' ' || lines[i][indent] == '\t'))
                {
                    indent++;
                }
                if (indent < minIndent)
                {
                    minIndent = indent;
                }
            }
        }

        if (minIndent == int.MaxValue || minIndent == 0)
        {
            return string.Join(Environment.NewLine, lines);
        }

        // Step 2: Strip the minimum indentation from non-heading lines
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("#"))
            {
                continue;
            }
            
            if (lines[i].Length >= minIndent && lines[i].Take(minIndent).All(c => c == ' ' || c == '\t'))
            {
                lines[i] = lines[i].Substring(minIndent);
            }
            else
            {
                lines[i] = lines[i].TrimStart(' ', '\t');
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}