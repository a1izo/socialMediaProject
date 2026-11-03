using System.Text;
using System.Text.RegularExpressions;

namespace ApiContracts;

// Rules for the "advanced" custom CSS on a profile.
// Sanitize() removes anything that could escape the page or load foreign content.
// Scope() prefixes every selector so the CSS only affects the profile container.
public static class ProfileCss
{
    public const int MaxLength = 20000;

    private static readonly Regex commentRegex = new(@"/\*.*?\*/", RegexOptions.Singleline);
    private static readonly Regex blockedAtRuleRegex = new(@"@(import|charset|namespace)[^;{]*;?", RegexOptions.IgnoreCase);
    private static readonly Regex fontFaceRegex = new(@"@font-face\s*\{[^}]*\}", RegexOptions.IgnoreCase);
    private static readonly Regex dangerousRegex = new(@"expression\s*\(|javascript\s*:|vbscript\s*:|behavior\s*:|-moz-binding",
        RegexOptions.IgnoreCase);
    private static readonly Regex urlRegex = new(@"url\(\s*(['""]?)(.*?)\1\s*\)", RegexOptions.IgnoreCase);
    private static readonly Regex allowedUrlRegex = new(@"^(https?://[a-zA-Z0-9.:-]+)?/uploads/[A-Za-z0-9._-]+$");
    private static readonly Regex fixedRegex = new(@"position\s*:\s*fixed", RegexOptions.IgnoreCase);

    public static string Sanitize(string? css)
    {
        if (string.IsNullOrWhiteSpace(css))
        {
            return "";
        }

        string result = css.Length > MaxLength ? css[..MaxLength] : css;
        result = commentRegex.Replace(result, "");
        // no tags (prevents closing the <style> element) and no escape sequences (prevents obfuscation)
        result = result.Replace("<", "").Replace(">", " > ").Replace("\\", "");
        result = blockedAtRuleRegex.Replace(result, "");
        result = fontFaceRegex.Replace(result, "");
        result = dangerousRegex.Replace(result, "");
        result = urlRegex.Replace(result, match =>
        {
            string url = match.Groups[2].Value.Trim();
            return allowedUrlRegex.IsMatch(url) ? $"url(\"{url}\")" : "none";
        });
        result = fixedRegex.Replace(result, "position: absolute");
        return result.Trim();
    }

    public static string Scope(string? css, string scopeSelector)
    {
        string sanitized = Sanitize(css);
        StringBuilder output = new();
        ScopeBlock(sanitized, scopeSelector, output);
        return output.ToString();
    }

    private static void ScopeBlock(string css, string scope, StringBuilder output)
    {
        int index = 0;
        while (index < css.Length)
        {
            int open = css.IndexOf('{', index);
            if (open < 0)
            {
                return;
            }

            string prelude = css[index..open].Trim().TrimStart(';', '}').Trim();
            int close = FindMatchingBrace(css, open);
            if (close < 0)
            {
                return;
            }

            string inner = css[(open + 1)..close];
            index = close + 1;

            if (prelude.StartsWith('@'))
            {
                string lower = prelude.ToLower();
                if (lower.StartsWith("@media") || lower.StartsWith("@supports") || lower.StartsWith("@container"))
                {
                    output.Append(prelude).Append(" {\n");
                    ScopeBlock(inner, scope, output);
                    output.Append("}\n");
                }
                else if (lower.Contains("keyframes"))
                {
                    output.Append(prelude).Append(" {").Append(inner).Append("}\n");
                }

                // any other at-rule (@page, @layer, ...) is dropped
                continue;
            }

            if (prelude.Length == 0)
            {
                continue;
            }

            IEnumerable<string> selectors = prelude.Split(',')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Select(s => ScopeSelector(s, scope));
            output.Append(string.Join(", ", selectors)).Append(" {").Append(inner.Replace("{", "")).Append("}\n");
        }
    }

    private static string ScopeSelector(string selector, string scope)
    {
        foreach (string root in new[] { ":root", "html", "body" })
        {
            if (selector.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return scope + selector[root.Length..];
            }
        }

        return selector.StartsWith('&') ? scope + selector[1..] : $"{scope} {selector}";
    }

    private static int FindMatchingBrace(string css, int open)
    {
        int depth = 0;
        for (int i = open; i < css.Length; i++)
        {
            if (css[i] == '{')
            {
                depth++;
            }
            else if (css[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }
}
