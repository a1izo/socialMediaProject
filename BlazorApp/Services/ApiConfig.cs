namespace BlazorApp.Services;

// Where the Web API lives. Images are stored as paths like "/uploads/x.png" on the API,
// so they must be resolved against the API address before the browser can load them.
public class ApiConfig
{
    public string BaseUrl { get; }

    public ApiConfig(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
    }

    public string Resolve(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "";
        }

        return url.StartsWith('/') ? BaseUrl + url : url;
    }
}
