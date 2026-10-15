using System.Collections.Generic;
using AngleSharp.Dom;

namespace AccessibilityAnalyser.Core.Rules;

// Finds headings that skip a level going deeper, e.g. an h1 followed straight by an h3.
public class HeadingOrderRule
{
    public IEnumerable<IElement> FindSkippedHeadings(IDocument document)
    {
        // comes back in document order, which is what we need
        var headings = document.QuerySelectorAll("h1, h2, h3, h4, h5, h6");

        int previousLevel = 0;
        foreach (var heading in headings)
        {
            // tag name is like "H3", so the second character is the level
            int level = heading.TagName[1] - '0';

            // previousLevel > 0 means the first heading on the page is never flagged
            if (previousLevel > 0 && level > previousLevel + 1)
                yield return heading;

            previousLevel = level;
        }
    }
}