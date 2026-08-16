using System;

namespace AccessabilityAnalyser;

public class Report
{
	double finalScore;
	string uri;
	public Report(string uri, string html)
	{
		uri = uri;
		finalScore = 0.0;
		var html = html;
	}

	public Report GenerateReport(string uri, ...)
	{
        var fetcher = new SourceFetcher();
        var html = await fetcher.GetHtmlAsync(uri);

		Report currentReport = new Report(uri, html);

		int AltsResult = AccessabilityAnalyser.altdetect ScanAsync(html);
		switch(AltsResult)
			case: 0
				currentReport.finalScore += 15.0;
				break;		
			case: 1
				currentReport.finalScore += 10.0
				break;
			case: >1
				currentReport.finalScore -= 5.0
				break;
			


    }
}
