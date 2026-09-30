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

public List<string> MissingLanguageIssues { get; private set; } = new();
	public List<string> MalformedHtmlIssues { get; private set; } = new();
	public List<FormElementMissingLabel> FormLabelIssues { get; private set; } = new();

	public List<string> DuplicateIdIssues { get; private set; } = new();

	public List<string> MissingPageTitleIssues { get; private set; } = new();

	public List<string> EmptyLinkIssues { get; private set; } = new();

	public List<ContrastFailure> ContrastFailures { get; private set; } = new();

	public int ButtonTextIssues { get; private set; }

	public List<ButtonTextFailure> ButtonTextIssueDetails { get; private set; } = new();

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

		// Run form label analysis
		var formLabelDetector = new FormLabelDetection();

		var formLabelIssues = await formLabelDetector.ScanAsync(html);

		report.FormLabelIssues = formLabelIssues;

		// Adjust score based on form label issues
		report.FinalScore -= Math.Min(
			20.0,
			formLabelIssues.Count * 2.0
		);

// Run malformed HTML analysis
var malformedDetector = new malformedHtml();

var malformedIssues = await malformedDetector.ScanAsync(html);

report.MalformedHtmlIssues = new List<string>(malformedIssues);

// Adjust score based on malformed HTML issues
report.FinalScore -= Math.Min(
    20.0,
    malformedIssues.Count * 2.0
);



// Run empty link analysis
var parser = new HtmlParser();
var document = await parser.ParseAsync(html);

var emptyLinkRule = new AccessibilityAnalyser.Core.Rules.EmptyLinkRule();

var emptyLinks = emptyLinkRule.FindEmptyLinks(document);

foreach (var link in emptyLinks)
{
    report.EmptyLinkIssues.Add(
        $"Empty link detected: <a href=\"{link.GetAttribute("href")}\"> has no accessible text or label."
    );
}

// Adjust score based on empty link issues
report.FinalScore -= Math.Min(
    20.0,
    report.EmptyLinkIssues.Count * 2.0
);

// Run duplicate ID analysis
var duplicateIdRule =
    new AccessibilityAnalyser.Core.Rules.DuplicateIdRule();

var duplicateIds = duplicateIdRule.FindDuplicateIds(document);

foreach (var group in duplicateIds)
{
    report.DuplicateIdIssues.Add(
        $"Duplicate ID detected: \"{group.Key}\" is used by {group.Count()} elements."
    );
}

// Adjust score based on duplicate ID issues
report.FinalScore -= Math.Min(
    20.0,
    report.DuplicateIdIssues.Count * 2.0
);

// BUTTON AND LINK TEXT
var buttonTextDetector = new ButtonTextDetection();

var buttonTextResult = await buttonTextDetector.ScanAsync(html);

report.ButtonTextIssues = buttonTextResult.Count;

report.ButtonTextIssueDetails =
    new List<ButtonTextFailure>(
        buttonTextResult.FailedElements
    );

report.FinalScore -= Math.Min(
    20.0,
    report.ButtonTextIssues * 2.0
);

// MISSING LANGUAGE
var missingLanguageRule =
    new AccessibilityAnalyser.Core.Rules.MissingLanguageRule();

if (missingLanguageRule.IsLanguageMissing(document))
{
    report.MissingLanguageIssues.Add(
        "The <html> element is missing a language attribute. " +
        "Recommendation: Add a lang attribute such as lang=\"en\" " +
        "to identify the language of the page."
    );
}

report.FinalScore -= Math.Min(
    20.0,
    report.MissingLanguageIssues.Count * 2.0
);

// MISSING PAGE TITLE
var missingPageTitleRule =
    new AccessibilityAnalyser.Core.Rules.MissingPageTitleRule();

if (missingPageTitleRule.IsTitleMissing(document))
{
    report.MissingPageTitleIssues.Add(
        "The page is missing a title. " +
        "Recommendation: Add a descriptive <title> element inside the <head> section."
    );
}

report.FinalScore -= Math.Min(
    20.0,
    report.MissingPageTitleIssues.Count * 2.0
);
		// Run contrast analysis
		var contrastFailures = await colourUtils.AnalyzeSiteContrastAsync(uri, html, 4.5);
		report.ContrastFailures = contrastFailures ?? new List<ContrastFailure>();

		// Adjust score based on contrast issues (simple heuristic)
		report.FinalScore -= Math.Min(20.0, report.ContrastFailures.Count * 1.5);

		return report;
	}
}