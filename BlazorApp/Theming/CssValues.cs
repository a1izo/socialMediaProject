using System.Text.RegularExpressions;

namespace BlazorApp.Theming;

// Theme values end up inside style="" attributes, so each one is checked against a strict
// pattern. Anything that does not match falls back to a safe default.
public static class CssValues
{
    private static readonly Regex colorRegex = new(
        @"^(#([0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})|(rgb|rgba|hsl|hsla)\([0-9.,%\s/deg]+\)|transparent)$");
    private static readonly Regex fontRegex = new(@"^[A-Za-z0-9 ]{1,40}$");
    private static readonly Regex urlRegex = new(@"^(https?://[^\s""'()<>\\]+|/[A-Za-z0-9/_.-]+)$");

    public static string Color(string? value, string fallback)
    {
        string trimmed = value?.Trim() ?? "";
        return colorRegex.IsMatch(trimmed) ? trimmed : fallback;
    }

    public static bool IsColor(string? value)
    {
        return value is not null && colorRegex.IsMatch(value.Trim());
    }

    public static string Font(string? value, string fallback)
    {
        string trimmed = value?.Trim() ?? "";
        return fontRegex.IsMatch(trimmed) ? trimmed : fallback;
    }

    public static bool IsUrl(string? value)
    {
        return value is not null && urlRegex.IsMatch(value.Trim());
    }

    public static int Clamp(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    public static string OneOf(string? value, string fallback, params string[] allowed)
    {
        return value is not null && allowed.Contains(value) ? value : fallback;
    }
}
