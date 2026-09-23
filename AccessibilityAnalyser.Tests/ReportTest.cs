using System;
using System.Collections.Generic;
using System.Text;
using AccessibilityAnalyser.Core;
using AngleSharp.Dom;

namespace AccessibilityAnalyser.Tests
{
    public class ReportTest
    {
        [Fact]
        public async Task KnownBadSite()
        {
            var report = await Report.GenerateReportAsync("https://www.midasgroup.online/constrast");
            Assert.True(report.ContrastFailures.Count >= 1,
                "The live page should still contain at least one low-contrast element as a smoke test.");
        }
    }
}
