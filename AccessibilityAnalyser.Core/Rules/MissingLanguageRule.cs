using AngleSharp.Dom;

namespace AccessibilityAnalyser.Core.Rules;

// Checks that the <html> element has a non-empty lang attribute.
// Screen readers use this to select the correct pronunciation and voice.
public class MissingLanguageRule
{
    public bool IsLanguageMissing(IDocument document)
    {
        var htmlElement = document.QuerySelector("html");
        var lang = htmlElement?.GetAttribute("lang");
        return string.IsNullOrWhiteSpace(lang);
    }
}