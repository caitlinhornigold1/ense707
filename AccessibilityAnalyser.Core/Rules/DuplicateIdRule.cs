using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;

namespace AccessibilityAnalyser.Core.Rules;

// Finds HTML ids that are used more than once on the page. duplicate ids can break label associations, ARIA references, and JS targeting.
public class DuplicateIdRule
{
    public IEnumerable<IGrouping<string, IElement>> FindDuplicateIds(IDocument document)
    {
        return document.QuerySelectorAll("[id]")
            .Where(el => !string.IsNullOrWhiteSpace(el.GetAttribute("id")))
            .GroupBy(el => el.GetAttribute("id")!)
            .Where(group => group.Count() > 1);
    }
}