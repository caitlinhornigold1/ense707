using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AccessibilityAnalyser.Core
{
    public class KeyboardDetection
    {
        public int KeyboardIssues { get; private set; }

        public List<string> IssueDescriptions { get; private set; } = new();

        public int Scan(string html)
        {
            int issues = 0;

            IssueDescriptions.Clear();

            issues += CheckPositiveTabIndex(html);
            issues += CheckNegativeTabIndex(html);
            issues += CheckClickableNonInteractiveElements(html);

            KeyboardIssues = issues;

            return issues;
        }

        private int CheckPositiveTabIndex(string html)
        {
            int issues = 0;

            var matches = Regex.Matches(
                html,
                @"<([a-zA-Z0-9]+)\b[^>]*\btabindex\s*=\s*([""'])(\d+)\2[^>]*>",
                RegexOptions.IgnoreCase
            );

            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[3].Value, out int tabindex))
                {
                    if (tabindex > 0)
                    {
                        issues++;

                        IssueDescriptions.Add(
                            $"Positive tabindex detected: <{match.Groups[1].Value}> uses tabindex=\"{tabindex}\". " +
                            "Positive tabindex values can create an unexpected keyboard navigation order. " +
                            "Recommendation: Use tabindex=\"0\" or native interactive elements instead."
                        );
                    }
                }
            }

            return issues;
        }

        private int CheckNegativeTabIndex(string html)
        {
            var matches = Regex.Matches(
                html,
                @"<(button|input|select|textarea|a)\b[^>]*\btabindex\s*=\s*([""'])-1\2[^>]*>",
                RegexOptions.IgnoreCase
            );

            foreach (Match match in matches)
            {
                IssueDescriptions.Add(
                    $"Interactive element with tabindex=\"-1\" detected: <{match.Groups[1].Value}>. " +
                    "This element cannot normally be reached using the Tab key. " +
                    "Recommendation: Make sure tabindex=\"-1\" is intentional and that users can still access the element when required."
                );
            }

            return matches.Count;
        }

        private int CheckClickableNonInteractiveElements(string html)
        {
            var matches = Regex.Matches(
                html,
                @"<(div|span|p|li|section|article)\b[^>]*\bonclick\s*=",
                RegexOptions.IgnoreCase
            );

            foreach (Match match in matches)
            {
                IssueDescriptions.Add(
                    $"Clickable non-interactive element detected: <{match.Groups[1].Value}> uses onclick. " +
                    "This element may not be keyboard accessible because it is not a native interactive element. " +
                    "Recommendation: Use a native <button> or <a> element instead."
                );
            }

            return matches.Count;
        }
    }
}
