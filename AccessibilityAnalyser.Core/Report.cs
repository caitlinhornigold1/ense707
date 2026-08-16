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
	public List<ContrastFailure> ContrastFailures { get; private set; } = new();

	public Report(string uri, string html)
	{
		Uri = uri;
		Html = html;
		FinalScore = 0.0;
		MissedAltAttributes = 0;
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

		if (missingAlts == 0)
			report.FinalScore += 15.0;
		else if (missingAlts == 1)
			report.FinalScore += 10.0;
		else
			report.FinalScore -= 5.0;

		// Run contrast analysis
		var contrastFailures = await colourUtils.AnalyzeSiteContrastAsync(uri, html, 4.5);
		report.ContrastFailures = contrastFailures ?? new List<ContrastFailure>();

		// Adjust score based on contrast issues (simple heuristic)
		report.FinalScore -= Math.Min(20.0, report.ContrastFailures.Count * 1.5);

		return report;
	}
}
