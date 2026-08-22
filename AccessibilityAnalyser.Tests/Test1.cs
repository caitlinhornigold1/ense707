using System.Linq;
using System.Threading.Tasks;
using AccessibilityAnalyser.Core;
//using AccessibilityAnalyser.altdetect;
using AngleSharp.Css.Dom;
using Xunit;
using AccessibilityAnalyser.Core.Rules;

namespace AccessibilityAnalyser.Tests;

    //[TestClass]
    public class ColourContrastTests
    {
        //[TestMethod]
        [Fact]
        public void WhiteTextBlackBackground()
        {
            Double value = colourUtils.GetContrastRatio("#FFFFFF", "#000000");
            Assert.Equal(21.0, value);
        }

        //[TestMethod]
        [Fact]
        public void SameColourBackgroundAndText()
        {
            Double value = colourUtils.GetContrastRatio("#FFFFFF", "#FFFFFF");
            Assert.Equal(1.0, value);
        }

        //[TestMethod]
        [Fact]
        public void ProperContrastButDifferentFormats()
        {
            Double value = colourUtils.GetContrastRatio("#000000", "rgb(255,255,255)");
            Assert.Equal(21.0, value);
        }
            [Fact]
    public async Task Fetcher_ThrowsAnalysisException_OnUnreachableHost()
    {
        var fetcher = new SourceFetcher();
        await Assert.ThrowsAsync<AnalysisException>(() =>
            fetcher.GetHtmlAsync("https://this-domain-should-not-exist-xyz123.com"));
    }
    }
