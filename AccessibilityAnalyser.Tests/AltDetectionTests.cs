using System;
using System.Collections.Generic;
using System.Text;
using AccessibilityAnalyser.Core;
using AngleSharp.Dom;

namespace AccessibilityAnalyser.Tests
{
    public class AltDetectionTests
    {
        [Fact]
        public async Task EmptyAlt()
        {
            String missingAltSite = "<html><body><img src='b.png' alt=''></body></html>";;
            var detector = new Detection();
		    int missingAlts = await detector.ScanAsync(missingAltSite);
            Assert.Equal(1, missingAlts);
        }

        [Fact]
        public async Task MissingAlt()
        {
            String missingAltSite = "<html><body>><img src='b.png'></html>";;
            var detector = new Detection();
		    int missingAlts = await detector.ScanAsync(missingAltSite);
            Assert.Equal(1, missingAlts);
        }
        

        [Fact]
        public async Task HasAlt()
        {
            String missingAltSite = "<html><body>><img src='b.png' alt='description'></html>";;
            var detector = new Detection();
		    int missingAlts = await detector.ScanAsync(missingAltSite);
            Assert.Equal(0, missingAlts);
        }
    }
}
