using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AccessibilityAnalyser.Core;

namespace AccessibilityAnalyser
{
    public class Detection
    {
        public int MissingAlts { get; private set; }

        public async Task<int> ScanAsync(string html)
        {
            // Get all <img>
            var imgMatches = Regex.Matches(html, @"<img\b[^>]*>", RegexOptions.IgnoreCase);
            int count = 0;

            foreach (Match img in imgMatches)
            {
                var imgTag = img.Value;

                // Match alt="value" or alt='value'
                var altMatch = Regex.Match(imgTag, @"alt\s*=\s*([""'])(.*?)\1", RegexOptions.IgnoreCase);

                if (!altMatch.Success)
                {
                    // No alt attribute present at all
                    count++;
                }
                else if (string.IsNullOrWhiteSpace(altMatch.Groups[2].Value))
                {
                    // Alt attribute exists but is empty
                    count++;
                }
            }

            MissingAlts = count;
            return count;
        }
    }
}