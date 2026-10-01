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


public int TestedCriteria { get; private set; }

public int PassedCriteria { get; private set; }

public int LevelATested { get; private set; }

public int LevelAPassed { get; private set; }

public int LevelAATested { get; private set; }

public int LevelAAPassed { get; private set; }
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

    public List<string> UnlabelledIframeIssues { get; private set; } = new();

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


		// Run keyboard accessibility analysis
		var keyboardDetector = new KeyboardDetection();
		int keyboardIssues = keyboardDetector.Scan(html);

		report.KeyboardIssues = keyboardIssues;
		report.KeyboardIssueDescriptions = new List<string>(
			keyboardDetector.IssueDescriptions
		);



		// Run responsive design analysis 
		var responsiveDetector = new ResponsiveDetection(); 
		int responsiveIssues = responsiveDetector.Scan(html); 
		report.ResponsiveIssues = responsiveIssues; 
		report.ResponsiveIssueDescriptions = new List<string>(responsiveDetector.IssueDescriptions); 

		// Run form label analysis
		var formLabelDetector = new FormLabelDetection();

		var formLabelIssues = await formLabelDetector.ScanAsync(html);

		report.FormLabelIssues = formLabelIssues;


// Run malformed HTML analysis
var malformedDetector = new malformedHtml();

var malformedIssues = await malformedDetector.ScanAsync(html);

report.MalformedHtmlIssues = new List<string>(malformedIssues);




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


// BUTTON AND LINK TEXT
var buttonTextDetector = new ButtonTextDetection();

var buttonTextResult = await buttonTextDetector.ScanAsync(html);

report.ButtonTextIssues = buttonTextResult.Count;

report.ButtonTextIssueDetails =
    new List<ButtonTextFailure>(
        buttonTextResult.FailedElements
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

// UNLABELLED IFRAMES
var unlabelledIframeRule =
    new AccessibilityAnalyser.Core.Rules.UnlabelledIframeRule();

var unlabelledIframes = unlabelledIframeRule.FindUnlabelledIframes(document);

foreach (var iframe in unlabelledIframes)
{
    report.UnlabelledIframeIssues.Add(
        $"Iframe with src \"{iframe.GetAttribute("src")}\" is missing a title attribute. " +
        "Recommendation: Add a descriptive title attribute so screen reader users know what the embedded content is."
    );
}


		// Run contrast analysis
		var contrastFailures = await colourUtils.AnalyzeSiteContrastAsync(uri, html, 4.5);
		report.ContrastFailures = contrastFailures ?? new List<ContrastFailure>();








// Calculate automated WCAG assessment score

// Level A criteria
report.TestedCriteria = 0;
report.PassedCriteria = 0;

report.LevelATested = 0;
report.LevelAPassed = 0;

report.LevelAATested = 0;
report.LevelAAPassed = 0;

// 1.1.1 Non-text Content - Level A
report.TestedCriteria++;
report.LevelATested++;

if (report.MissedAltAttributes == 0)
{
    report.PassedCriteria++;
    report.LevelAPassed++;
}

// 1.3.1 Info and Relationships - Level A
report.TestedCriteria++;
report.LevelATested++;

if (report.FormLabelIssues.Count == 0)
{
    report.PassedCriteria++;
    report.LevelAPassed++;
}

// 2.1.1 Keyboard - Level A
report.TestedCriteria++;
report.LevelATested++;

if (report.KeyboardIssues == 0)
{
    report.PassedCriteria++;
    report.LevelAPassed++;
}

// 2.4.2 Page Titled - Level A
report.TestedCriteria++;
report.LevelATested++;

if (report.MissingPageTitleIssues.Count == 0)
{
    report.PassedCriteria++;
    report.LevelAPassed++;
}

// 2.4.4 Link Purpose - Level A
report.TestedCriteria++;
report.LevelATested++;

if (report.EmptyLinkIssues.Count == 0)
{
    report.PassedCriteria++;
    report.LevelAPassed++;
}

// 3.1.1 Language of Page - Level A
report.TestedCriteria++;
report.LevelATested++;

if (report.MissingLanguageIssues.Count == 0)
{
    report.PassedCriteria++;
    report.LevelAPassed++;
}

// 4.1.2 Name, Role, Value - Level A
report.TestedCriteria++;
report.LevelATested++;

if (report.ButtonTextIssues == 0 && report.UnlabelledIframeIssues.Count == 0)
{
    report.PassedCriteria++;
    report.LevelAPassed++;
}

// 1.4.3 Contrast (Minimum) - Level AA
report.TestedCriteria++;
report.LevelAATested++;

if (report.ContrastFailures.Count == 0)
{
    report.PassedCriteria++;
    report.LevelAAPassed++;
}

// 1.4.10 Reflow - Level AA
report.TestedCriteria++;
report.LevelAATested++;

if (report.ResponsiveIssues == 0)
{
    report.PassedCriteria++;
    report.LevelAAPassed++;
}

// Calculate percentage
if (report.TestedCriteria > 0)
{
    report.FinalScore =
        (double)report.PassedCriteria /
        report.TestedCriteria *
        100.0;
}
else
{
    report.FinalScore = 0.0;
}

		return report;
	}
}