using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AccessibilityAnalyser.Core
{
    public class malformedHtml
    {
        private static readonly Regex TagRegex = new(
            @"<!--.*?-->|<(?<closing>/)?\s*(?<name>[A-Za-z][A-Za-z0-9:-]*)(?<attributes>[^<>]*?)\s*(?<selfClosing>/)?>",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly HashSet<string> VoidElements = new(StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input",
            "link", "meta", "param", "source", "track", "wbr"
        };

        public Task<IReadOnlyList<string>> ScanAsync(string html)
        {
            var openElements = new Stack<string>();
            var unclosedElements = new List<string>();

            foreach (Match tag in TagRegex.Matches(html ?? string.Empty))
            {
                if (tag.Value.StartsWith("<!--", System.StringComparison.Ordinal))
                {
                    continue;
                }

                string elementName = tag.Groups["name"].Value;
                bool isClosingTag = tag.Groups["closing"].Success;
                bool isSelfClosingTag = tag.Groups["selfClosing"].Success;

                if (!isClosingTag)
                {
                    if (!isSelfClosingTag && !VoidElements.Contains(elementName))
                    {
                        openElements.Push(elementName);
                    }

                    continue;
                }
                
                if (openElements.Count == 0)
                {
                    continue;
                }
                
                if (string.Equals(openElements.Peek(), elementName, System.StringComparison.OrdinalIgnoreCase))
                {
                    openElements.Pop();
                    continue;
                }

                // A closing tag can close any still-open descendants
                // but they remain unclosed and so should be reported
                while (openElements.Count > 0 &&
                       !string.Equals(openElements.Peek(), elementName, System.StringComparison.OrdinalIgnoreCase))
                {
                    unclosedElements.Add(openElements.Pop());
                }

                if (openElements.Count > 0)
                {
                    openElements.Pop();
                }
            }

            while (openElements.Count > 0)
            {
                unclosedElements.Add(openElements.Pop());
            }

            // when provided <div><p><span>text</div>
            // new List<string> { "span", "p" }
            return Task.FromResult<IReadOnlyList<string>>(unclosedElements);
        }
    }
}