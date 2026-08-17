using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;

namespace AccessibilityAnalyser.Core.Rules;

// Finds <a> elements that have no accessible text or label,
// e.g. <a href="..."></a> or <a href="..."><img></a> with no alt text
public class EmptyLinkRule
{
    public IEnumerable<IElement> FindEmptyLinks(IDocument document)
    {
        return document.QuerySelectorAll("a").Where(link => !HasAccessibleText(link));
    }

    private bool HasAccessibleText(IElement link)
    {
        var text = link.TextContent?.Trim();
        if (!string.IsNullOrEmpty(text))
            return true;

        var ariaLabel = link.GetAttribute("aria-label");
        if (!string.IsNullOrWhiteSpace(ariaLabel))
            return true;

        var title = link.GetAttribute("title");
        if (!string.IsNullOrWhiteSpace(title))
            return true;

        // a link with only an image inside can still be accessible if the image has alt text
        var img = link.QuerySelector("img");
        if (img != null && !string.IsNullOrWhiteSpace(img.GetAttribute("alt")))
            return true;

        return false;
    }
}