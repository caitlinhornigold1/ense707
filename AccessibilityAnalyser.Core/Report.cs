using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccessibilityAnalyser;

namespace AccessibilityAnalyser.Core;

public class Report
{
	public string Uri { get; init; }
	public string Html { get; init; }
	public double FinalScore { get; private set; }
	public int MissedAltAttributes { get; private set; }
	public List<string> AltTextIssueDescriptions { get; private set; } = new();
	public int KeyboardIssues { get; private set; }
	public List<string> KeyboardIssueDescriptions { get; private set; } = new();

	public int ResponsiveIssues { get; private set; }
	public List<string> ResponsiveIssueDescriptions { get; private set; } = new();
	public List<ContrastFailure> ContrastFailures { get; private set; } = new();

	public Report(string uri, string html)
	{
		Uri = uri;
		Html = html;
		FinalScore = 0.0;
		MissedAltAttributes = 0;
		KeyboardIssues = 0;
		ResponsiveIssues = 0;
	}

	public static async Task<Report> GenerateReportAsync(string uri)
	{
		var fetcher = new SourceFetcher();
		var html = await fetcher.GetHtmlAsync(uri);

		var report = new Report(uri, html);

		// Run alt-text detection (separate project/class)
		var detector = new Detection();
		int missingAlts = await detector.ScanAsync(html);
		report.MissedAltAttributes = missingAlts;

		var altMatches = System.Text.RegularExpressions.Regex.Matches(
    html,
    @"<img\b[^>]*>",
    System.Text.RegularExpressions.RegexOptions.IgnoreCase
);

foreach (System.Text.RegularExpressions.Match match in altMatches)
{
    var imgTag = match.Value;

    var altMatch = System.Text.RegularExpressions.Regex.Match(
        imgTag,
        @"alt\s*=\s*([""'])(.*?)\1",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase
    );

    if (!altMatch.Success)
    {
        report.AltTextIssueDescriptions.Add(
            "Image is missing an alt attribute. Recommendation: Add appropriate alternative text describing the image."
        );
    }
    else if (string.IsNullOrWhiteSpace(altMatch.Groups[2].Value))
    {
        report.AltTextIssueDescriptions.Add(
            "Image has an empty alt attribute. Recommendation: Add descriptive alternative text, unless the image is purely decorative."
        );
    }
}

		if (missingAlts == 0)
			report.FinalScore += 15.0;
		else if (missingAlts == 1)
			report.FinalScore += 10.0;
		else
			report.FinalScore -= 5.0;

		// Run keyboard accessibility analysis
		var keyboardDetector = new KeyboardDetection();
		int keyboardIssues = keyboardDetector.Scan(html);

		report.KeyboardIssues = keyboardIssues;
		report.KeyboardIssueDescriptions = new List<string>(
			keyboardDetector.IssueDescriptions
		);

		// Adjust score based on keyboard issues
		report.FinalScore -= Math.Min(20.0, keyboardIssues * 2.0);

		// Run responsive design analysis 
		var responsiveDetector = new ResponsiveDetection(); 
		int responsiveIssues = responsiveDetector.Scan(html); 
		report.ResponsiveIssues = responsiveIssues; 
		report.ResponsiveIssueDescriptions = new List<string>(responsiveDetector.IssueDescriptions); 
		// Adjust score based on responsive issues 
		report.FinalScore -= Math.Min(20.0, responsiveIssues * 2.0);

		// Run contrast analysis
		var contrastFailures = await colourUtils.AnalyzeSiteContrastAsync(uri, html, 4.5);
		report.ContrastFailures = contrastFailures ?? new List<ContrastFailure>();

		// Adjust score based on contrast issues (simple heuristic)
		report.FinalScore -= Math.Min(20.0, report.ContrastFailures.Count * 1.5);

		return report;
	}
}