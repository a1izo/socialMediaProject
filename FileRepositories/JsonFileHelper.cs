using System.Text.Json;

namespace FileRepositories;

internal static class JsonFileHelper
{
    public static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    public static List<T> Load<T>(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    public static async Task<List<T>> LoadAsync<T>(string filePath)
    {
        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    public static async Task SaveAsync<T>(string filePath, List<T> list)
    {
        string json = JsonSerializer.Serialize(list);
        await File.WriteAllTextAsync(filePath, json);
    }
}
