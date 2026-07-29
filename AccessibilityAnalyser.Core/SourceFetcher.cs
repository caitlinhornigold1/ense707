using System.Net.Http;
using System.Threading.Tasks;

namespace AccessibilityAnalyser.Core;

// Downloads the raw HTML for a given URL so it can be parsed
public class SourceFetcher
{
    private readonly HttpClient _client = new();

    public SourceFetcher()
    {
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("AccessibilityAnalyser/1.0");
    }

    public async Task<string> GetHtmlAsync(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}