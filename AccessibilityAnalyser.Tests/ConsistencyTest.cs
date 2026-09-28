using AccessibilityAnalyser.Core;
using Xunit;

namespace AccessibilityAnalyser.Tests
{
    public class ConsistencyTest
    {
        [Fact]
        public async Task SameWebsite_ShouldProduceConsistentResults()
        {
            var url = "https://www.midasgroup.online/constrast";

            var firstReport = await Report.GenerateReportAsync(url);
            var secondReport = await Report.GenerateReportAsync(url);

            Assert.Equal(
                firstReport.MissedAltAttributes,
                secondReport.MissedAltAttributes);

            Assert.Equal(
                firstReport.ContrastFailures.Count,
                secondReport.ContrastFailures.Count);

            Assert.Equal(
                firstReport.KeyboardIssues,
                secondReport.KeyboardIssues);

            Assert.Equal(
                firstReport.ResponsiveIssues,
                secondReport.ResponsiveIssues);

            Assert.Equal(
                firstReport.FinalScore,
                secondReport.FinalScore);
        }
    }
}