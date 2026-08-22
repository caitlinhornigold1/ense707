using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Css;
using AngleSharp.Dom;
using System;

namespace AccessibilityAnalyser.Core;

// Turns raw HTML into a queryable DOM document, with CSS support enabled
public class HtmlParser
{
    public async Task<IDocument> ParseAsync(string html)
    {
        try
        {
            var config = Configuration.Default.WithCss();
            var context = BrowsingContext.New(config);
            return await context.OpenAsync(req => req.Content(html));
        }
        catch (Exception ex)
        {
            throw new AnalysisException("The page could not be parsed. It may not be valid HTML.", ex);
        }
    }
}