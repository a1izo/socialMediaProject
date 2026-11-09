namespace BlazorApp.Services;

public static class TimeFormat
{
    public static string Ago(DateTime date)
    {
        TimeSpan span = DateTime.Now - date;
        if (span.TotalMinutes < 1) return "now";
        if (span.TotalHours < 1) return $"{(int)span.TotalMinutes}m";
        if (span.TotalDays < 1) return $"{(int)span.TotalHours}h";
        if (span.TotalDays < 30) return $"{(int)span.TotalDays}d";
        return date.ToString("dd MMM yyyy").ToLower();
    }

    public static string Stamp(DateTime date)
    {
        return date.ToString("d MMM yyyy, HH:mm").ToLower();
    }
}
