using System.Linq;
using System.Threading.Tasks;
using AccessibilityAnalyser.Core;
using AngleSharp.Css.Dom;
using Xunit;

namespace AccessibilityAnalyser.Tests;

public class ParsingTests
{
    [Fact]
    public async Task Fetcher_RetrievesHtml_FromLiveSite()
    {
        var fetcher = new SourceFetcher();
        var html = await fetcher.GetHtmlAsync("https://www.google.com");
        Assert.Contains("<html", html, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Parser_CanQueryElements_FromHtml()
    {
        var html = "<html><body><img src='a.png'><img src='b.png' alt='desc'></body></html>";
        var parser = new HtmlParser();
        var doc = await parser.ParseAsync(html);
        var images = doc.QuerySelectorAll("img");
        Assert.Equal(2, images.Length);
    }

    [Fact]
    public async Task Parser_ReadsColourAndSizingFromCss()
    {
        var html = @"
            <html>
            <head><style>
                .box { color: #333333; background-color: #ffffff; font-size: 16px; width: 200px; }
            </style></head>
            <body><div class='box'>Hello</div></body>
            </html>";

        var parser = new HtmlParser();
        var doc = await parser.ParseAsync(html);

        var style = doc.StyleSheets
            .OfType<ICssStyleSheet>()
            .SelectMany(s => s.Rules.OfType<ICssStyleRule>())
            .First(r => r.SelectorText == ".box")
            .Style;

        Assert.Equal("rgba(51, 51, 51, 1)", style.GetPropertyValue("color"));
        Assert.Equal("rgba(255, 255, 255, 1)", style.GetPropertyValue("background-color"));
        Assert.Equal("16px", style.GetPropertyValue("font-size"));
        Assert.Equal("200px", style.GetPropertyValue("width"));
    }
}