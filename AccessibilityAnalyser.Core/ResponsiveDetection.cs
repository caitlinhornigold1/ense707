using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AccessibilityAnalyser.Core
{
    public class ResponsiveDetection
    {
        public int ResponsiveIssues { get; private set; }

        public List<string> IssueDescriptions { get; private set; } = new();

        public int Scan(string html)
        {
            int issues = 0;

            IssueDescriptions.Clear();

            issues += CheckViewportMeta(html);
            issues += CheckFixedWidths(html);
            issues += CheckFixedMinWidths(html);
            issues += CheckFixedImageWidths(html);
            issues += CheckMediaQueries(html);

            ResponsiveIssues = issues;

            return issues;
        }

        private int CheckViewportMeta(string html)
        {
            var viewportMatch = Regex.Match(
                html,
                @"<meta\b[^>]*\bname\s*=\s*[""']viewport[""'][^>]*>",
                RegexOptions.IgnoreCase
            );

            if (!viewportMatch.Success)
            {
                IssueDescriptions.Add(
                    "Viewport meta tag is missing. " +
                    "This may prevent the website from adapting correctly to mobile screen sizes. " +
                    "Recommendation: Add a viewport meta tag such as " +
                    "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">."
                );

                return 1;
            }

            return 0;
        }

        private int CheckFixedWidths(string html)
        {
            int issues = 0;

            // Find regular CSS blocks.
            var cssBlocks = Regex.Matches(
                html,
                @"(?<selector>[^{]+)\{(?<styles>[^{}]*)\}",
                RegexOptions.IgnoreCase
            );

            // Find mobile media-query blocks.
            var mobileBlocks = GetMobileMediaQueryBlocks(html);

            foreach (Match block in cssBlocks)
            {
                string selector = block.Groups["selector"].Value.Trim();
                string styles = block.Groups["styles"].Value;

                // Skip media-query declarations themselves.
                if (selector.Contains("@media"))
                    continue;

                var widthMatch = Regex.Match(
                    styles,
                    @"\bwidth\s*:\s*(\d+)px\s*;",
                    RegexOptions.IgnoreCase
                );

                if (!widthMatch.Success)
                    continue;

                if (!int.TryParse(widthMatch.Groups[1].Value, out int width))
                    continue;

                if (width <= 768)
                    continue;

                // Check whether this selector has a width override inside a mobile media query.
                if (HasMobileOverride(mobileBlocks, selector, "width"))
                    continue;

                issues++;

                IssueDescriptions.Add(
                    $"Potential responsive issue: {selector} has a fixed width of {width}px " +
                    "with no detected mobile override. This may cause horizontal scrolling " +
                    "on smaller screens. " +
                    "Recommendation: Override the width at smaller screen sizes or use a flexible " +
                    "value such as %, vw, or max-width."
                );
            }

            return issues;
        }

        private int CheckFixedMinWidths(string html)
        {
            int issues = 0;

            var cssBlocks = Regex.Matches(
                html,
                @"(?<selector>[^{]+)\{(?<styles>[^{}]*)\}",
                RegexOptions.IgnoreCase
            );

            var mobileBlocks = GetMobileMediaQueryBlocks(html);

            foreach (Match block in cssBlocks)
            {
                string selector = block.Groups["selector"].Value.Trim();
                string styles = block.Groups["styles"].Value;

                if (selector.Contains("@media"))
                    continue;

                var minWidthMatch = Regex.Match(
                    styles,
                    @"\bmin-width\s*:\s*(\d+)px\s*;",
                    RegexOptions.IgnoreCase
                );

                if (!minWidthMatch.Success)
                    continue;

                if (!int.TryParse(minWidthMatch.Groups[1].Value, out int width))
                    continue;

                if (width <= 768)
                    continue;

                if (HasMobileOverride(mobileBlocks, selector, "min-width"))
                    continue;

                issues++;

                IssueDescriptions.Add(
                    $"Potential responsive issue: {selector} has a min-width of {width}px " +
                    "with no detected mobile override. This may prevent the content from " +
                    "shrinking on smaller screens. " +
                    "Recommendation: Override the min-width at smaller screen sizes where appropriate."
                );
            }

            return issues;
        }

        private int CheckFixedImageWidths(string html)
        {
            int issues = 0;

            var matches = Regex.Matches(
                html,
                @"<img\b[^>]*\bwidth\s*=\s*[""'](\d+)[""'][^>]*>",
                RegexOptions.IgnoreCase
            );

            foreach (Match match in matches)
            {
                if (!int.TryParse(match.Groups[1].Value, out int width))
                    continue;

                if (width <= 768)
                    continue;

                issues++;

                IssueDescriptions.Add(
                    $"Potential responsive issue: an image has a fixed width of {width}px. " +
                    "This may cause the image to extend beyond the viewport on smaller screens. " +
                    "Recommendation: Consider using responsive image sizing such as max-width: 100%."
                );
            }

            return issues;
        }

        private int CheckMediaQueries(string html)
        {
            var matches = Regex.Matches(
                html,
                @"@media\s*(?:[^{]+)\{",
                RegexOptions.IgnoreCase
            );

            if (matches.Count == 0)
            {
                IssueDescriptions.Add(
                    "No CSS media queries were detected. " +
                    "This does not necessarily mean the website is not responsive, " +
                    "but media queries are commonly used to adapt layouts for different screen sizes. " +
                    "Recommendation: Check that the layout adapts appropriately to smaller screens."
                );

                return 1;
            }

            return 0;
        }

        private List<(string Condition, string Content)> GetMobileMediaQueryBlocks(string html)
        {
            var results = new List<(string Condition, string Content)>();

            var matches = Regex.Matches(
                html,
                @"@media\s*(?<condition>[^{]+)\{(?<content>.*?)\}",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            foreach (Match match in matches)
            {
                string condition = match.Groups["condition"].Value.Trim();

                // Look for max-width media queries.
                if (Regex.IsMatch(
                    condition,
                    @"max-width\s*:\s*\d+px",
                    RegexOptions.IgnoreCase))
                {
                    results.Add((
                        condition,
                        match.Groups["content"].Value
                    ));
                }
            }

            return results;
        }

        private bool HasMobileOverride(
            List<(string Condition, string Content)> mobileBlocks,
            string selector,
            string property)
        {
            foreach (var block in mobileBlocks)
            {
                var cssBlockMatch = Regex.Match(
                    block.Content,
                    Regex.Escape(selector) + @"\s*\{(?<styles>[^{}]*)\}",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline
                );

                if (!cssBlockMatch.Success)
                    continue;

                string styles = cssBlockMatch.Groups["styles"].Value;

                if (Regex.IsMatch(
                    styles,
                    $@"\b{Regex.Escape(property)}\s*:",
                    RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
