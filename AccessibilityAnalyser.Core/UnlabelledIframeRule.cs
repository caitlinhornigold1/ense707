using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;

namespace AccessibilityAnalyser.Core.Rules;
// Finds <iframe> elements with no title attribute.
public class UnlabelledIframeRule
{
    public IEnumerable<IElement> FindUnlabelledIframes(IDocument document)
    {
        return document.QuerySelectorAll("iframe")
            .Where(iframe => string.IsNullOrWhiteSpace(iframe.GetAttribute("title")));
    }
}