using AccessibilityAnalyser.Core;
var url = args.Length > 0 ? args[0] : "https://www.google.com";
try
{
    var fetcher = new SourceFetcher();
    var parser = new HtmlParser();

    Console.WriteLine($"Fetching: {url}");
    var html = await fetcher.GetHtmlAsync(url);
    var document = await parser.ParseAsync(html);
    var imageCount = document.QuerySelectorAll("img").Length;
    Console.WriteLine($"Page title: {document.Title}");
    Console.WriteLine($"Images found: {imageCount}");
}
catch (AnalysisException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}