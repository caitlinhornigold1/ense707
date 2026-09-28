using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccessibilityAnalyser.Core;

public class FormElementMissingLabel
{
    public string TagName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class FormLabelDetection
{
    // Entry point
    public async Task<List<FormElementMissingLabel>> ScanAsync(string html)
    {
        var missingLabels = new List<FormElementMissingLabel>();
        var document = await new HtmlParser().ParseAsync(html);

        // Iterate through form elements
        foreach (var element in document.QuerySelectorAll("input, select, textarea, button"))
        {
            var type = element.GetAttribute("type") ?? string.Empty;

            if (element.LocalName == "input" &&
                (type.Equals("hidden", System.StringComparison.OrdinalIgnoreCase) ||
                 type.Equals("submit", System.StringComparison.OrdinalIgnoreCase) ||
                 type.Equals("reset", System.StringComparison.OrdinalIgnoreCase) ||
                 type.Equals("button", System.StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (HasAccessibleLabel(element, document))
            {
                continue;
            }

            missingLabels.Add(new FormElementMissingLabel
            {
                TagName = element.LocalName,
                Type = type,
                Id = element.GetAttribute("id") ?? string.Empty,
                Name = element.GetAttribute("name") ?? string.Empty
            });
        }

        return missingLabels;
    }

    private static bool HasAccessibleLabel(AngleSharp.Dom.IElement element, AngleSharp.Dom.IDocument document)
    {
        if (!string.IsNullOrWhiteSpace(element.GetAttribute("aria-label")) ||
            HasReferencedLabel(element, document))
        {
            return true;
        }

        if (element.LocalName == "input" &&
            element.GetAttribute("type")?.Equals("image", System.StringComparison.OrdinalIgnoreCase) == true &&
            !string.IsNullOrWhiteSpace(element.GetAttribute("alt")))
        {
            return true;
        }

        if (element.LocalName == "button" && HasAccessibleText(element))
        {
            return true;
        }

        var id = element.GetAttribute("id");
        if (!string.IsNullOrWhiteSpace(id) &&
            document.QuerySelectorAll("label[for]").Any(label =>
                label.GetAttribute("for") == id && HasAccessibleText(label)))
        {
            return true;
        }

        var ancestor = element.ParentElement;
        while (ancestor != null)
        {
            if (ancestor.LocalName == "label" && HasAccessibleText(ancestor))
            {
                return true;
            }

            ancestor = ancestor.ParentElement;
        }

        return false;
    }

    private static bool HasReferencedLabel(AngleSharp.Dom.IElement element, AngleSharp.Dom.IDocument document)
    {
        var labelledBy = element.GetAttribute("aria-labelledby");
        if (string.IsNullOrWhiteSpace(labelledBy))
        {
            return false;
        }

        return labelledBy.Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(id => document.GetElementById(id))
            .Any(referenced => referenced != null && HasAccessibleText(referenced));
    }

    private static bool HasAccessibleText(AngleSharp.Dom.IElement element)
    {
        if (!string.IsNullOrWhiteSpace(element.TextContent) ||
            !string.IsNullOrWhiteSpace(element.GetAttribute("aria-label")))
        {
            return true;
        }

        return element.QuerySelectorAll("img[alt]")
            .Any(image => !string.IsNullOrWhiteSpace(image.GetAttribute("alt")));
    }
}