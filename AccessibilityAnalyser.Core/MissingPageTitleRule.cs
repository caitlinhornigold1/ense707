using AngleSharp.Dom;

namespace AccessibilityAnalyser.Core.Rules;

// Checks that the page has a non-empty <title> element.
public class MissingPageTitleRule
{
    public bool IsTitleMissing(IDocument document)
    {
        var title = document.Title;
        return string.IsNullOrWhiteSpace(title);
    }
}