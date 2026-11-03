using System.Text.Json;

namespace BlazorApp.Services;

internal static class HttpResponseReader
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T> ReadAsync<T>(HttpResponseMessage httpResponse)
    {
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(ExtractError(response, httpResponse));
        }

        return JsonSerializer.Deserialize<T>(response, Options)!;
    }

    public static async Task EnsureSuccessAsync(HttpResponseMessage httpResponse)
    {
        if (!httpResponse.IsSuccessStatusCode)
        {
            string response = await httpResponse.Content.ReadAsStringAsync();
            throw new Exception(ExtractError(response, httpResponse));
        }
    }

    // The Web API answers errors as { status, error }, as validation problems { title, errors },
    // or as a plain string. Turn all of them into one readable message.
    private static string ExtractError(string response, HttpResponseMessage httpResponse)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return $"{(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}";
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(response);
            JsonElement root = document.RootElement;
            if (root.ValueKind == JsonValueKind.String)
            {
                return root.GetString()!;
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("error", out JsonElement error))
                {
                    return error.GetString() ?? response;
                }

                if (root.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    IEnumerable<string> messages = errors.EnumerateObject()
                        .SelectMany(e => e.Value.EnumerateArray().Select(v => v.GetString() ?? ""));
                    return string.Join(" ", messages);
                }

                if (root.TryGetProperty("title", out JsonElement title))
                {
                    return title.GetString() ?? response;
                }
            }
        }
        catch (JsonException)
        {
            // not json, fall through to the raw text
        }

        return response;
    }
}
