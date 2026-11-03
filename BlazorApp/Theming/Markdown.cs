using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Theming;

// A small, safe markdown subset: headings, bold, italic, strike, inline code, links,
// lists, quotes and rules. Everything is HTML-encoded first, so user input can never
// produce its own tags. Only http(s) links are turned into anchors.
public static class Markdown
{
    private static readonly Regex codeRegex = new(@"`([^`]+)`");
    private static readonly Regex boldRegex = new(@"\*\*(.+?)\*\*");
    private static readonly Regex italicRegex = new(@"(?<![*\w])\*(?!\s)(.+?)(?<!\s)\*(?!\*)");
    private static readonly Regex strikeRegex = new(@"~~(.+?)~~");
    private static readonly Regex linkRegex = new(@"\[([^\]]+)\]\((https?://[^\s)]+)\)");

    public static MarkupString Render(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new MarkupString("");
        }

        StringBuilder html = new();
        string? openList = null;
        List<string> paragraph = new();

        void FlushParagraph()
        {
            if (paragraph.Count > 0)
            {
                html.Append("<p>").Append(string.Join("<br />", paragraph)).Append("</p>");
                paragraph.Clear();
            }
        }

        void CloseList()
        {
            if (openList is not null)
            {
                html.Append($"</{openList}>");
                openList = null;
            }
        }

        foreach (string rawLine in text.Replace("\r", "").Split('\n'))
        {
            string line = rawLine.TrimEnd();
            string trimmed = line.TrimStart();

            if (trimmed.Length == 0)
            {
                FlushParagraph();
                CloseList();
                continue;
            }

            if (trimmed is "---" or "***")
            {
                FlushParagraph();
                CloseList();
                html.Append("<hr />");
                continue;
            }

            Match heading = Regex.Match(trimmed, @"^(#{1,3})\s+(.*)$");
            if (heading.Success)
            {
                FlushParagraph();
                CloseList();
                int level = heading.Groups[1].Length;
                html.Append($"<h{level + 1} class=\"md-h{level}\">{Inline(heading.Groups[2].Value)}</h{level + 1}>");
                continue;
            }

            if (trimmed.StartsWith("> "))
            {
                FlushParagraph();
                CloseList();
                html.Append($"<blockquote>{Inline(trimmed[2..])}</blockquote>");
                continue;
            }

            Match bullet = Regex.Match(trimmed, @"^[-*+]\s+(.*)$");
            Match numbered = Regex.Match(trimmed, @"^\d+[.)]\s+(.*)$");
            if (bullet.Success || numbered.Success)
            {
                FlushParagraph();
                string listTag = bullet.Success ? "ul" : "ol";
                if (openList != listTag)
                {
                    CloseList();
                    html.Append($"<{listTag}>");
                    openList = listTag;
                }

                html.Append($"<li>{Inline((bullet.Success ? bullet : numbered).Groups[1].Value)}</li>");
                continue;
            }

            CloseList();
            paragraph.Add(Inline(trimmed));
        }

        FlushParagraph();
        CloseList();
        return new MarkupString(html.ToString());
    }

    // plain text version for excerpts
    public static string ToPlainText(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        string plain = Regex.Replace(text, @"[#*_`>~\[\]]|\(https?://[^)]*\)", "");
        plain = Regex.Replace(plain, @"\s+", " ").Trim();
        return plain.Length <= maxLength ? plain : plain[..maxLength].TrimEnd() + "…";
    }

    private static string Inline(string text)
    {
        string encoded = WebUtility.HtmlEncode(text);
        encoded = codeRegex.Replace(encoded, "<code>$1</code>");
        encoded = linkRegex.Replace(encoded, "<a href=\"$2\" target=\"_blank\" rel=\"noopener noreferrer nofollow\">$1</a>");
        encoded = boldRegex.Replace(encoded, "<strong>$1</strong>");
        encoded = italicRegex.Replace(encoded, "<em>$1</em>");
        encoded = strikeRegex.Replace(encoded, "<s>$1</s>");
        return encoded;
    }
}
