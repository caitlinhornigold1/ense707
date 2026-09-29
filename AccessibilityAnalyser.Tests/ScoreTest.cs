using AccessibilityAnalyser.Core;

namespace AccessibilityAnalyser.Tests
{
    public class ScoreTest
    {
        [Fact]
        public async Task GenerateAccessibilityScore()
        {
            var report = await Report.GenerateReportAsync(
                "https://www.midasgroup.online/constrast");

            Assert.True(
                report.FinalScore >= -50 && report.FinalScore <= 100,
                "The accessibility score should be between -50 and 100.");
        }
    }
}