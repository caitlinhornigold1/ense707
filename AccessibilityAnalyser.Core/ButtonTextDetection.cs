using System.Linq;
using System.Threading.Tasks;

namespace AccessibilityAnalyser.Core
{
    public sealed class ButtonTextFailure
    {
        public string ElementName { get; init; } = string.Empty;
        public string? Id { get; init; }
    }

    public sealed class ButtonTextDetectionResult
    {
        public int Count { get; init; }
        public System.Collections.Generic.List<ButtonTextFailure> FailedElements { get; init; } = new();
    }

    public class ButtonTextDetection
    {
        public async Task<ButtonTextDetectionResult> ScanAsync(string html)
        {
            var document = await new HtmlParser().ParseAsync(html);
            var failedElements = document.QuerySelectorAll("a, button")
                .Where(element => !HasAccessibleText(element, document))
                .Select(element => new ButtonTextFailure
                {
                    ElementName = element.LocalName,
                    Id = element.GetAttribute("id")
                })
                .ToList();

            return new ButtonTextDetectionResult
            {
                Count = failedElements.Count,
                FailedElements = failedElements
            };
        }

        // Checks if the element has accessible text, either directly or through attributes
        private static bool HasAccessibleText(AngleSharp.Dom.IElement element, AngleSharp.Dom.IDocument document)
        {
            if (!string.IsNullOrWhiteSpace(element.TextContent) ||
                !string.IsNullOrWhiteSpace(element.GetAttribute("aria-label")) ||
                !string.IsNullOrWhiteSpace(element.GetAttribute("title")))
            {
                return true;
            }

            var labelledBy = element.GetAttribute("aria-labelledby");
            if (!string.IsNullOrWhiteSpace(labelledBy) &&
                labelledBy.Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(document.GetElementById)
                    .Any(referenced => referenced != null && !string.IsNullOrWhiteSpace(referenced.TextContent)))
            {
                return true;
            }

            // if button uses image with alt text, I rarely see this outside of ads but for safety its here.
            return element.QuerySelectorAll("img[alt]")
                .Any(image => !string.IsNullOrWhiteSpace(image.GetAttribute("alt")));
        }
    }
}