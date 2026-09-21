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
        public void MissingTextColourUsesBrowserDefaultBlack()
        {
            Double value = colourUtils.GetContrastRatio(string.Empty, "rgb(255,255,255)");
            Assert.Equal(21.0, value);
        }

        [Fact]
        public async Task UnstyledTextIsNotReportedAsAContrastFailure()
        {
            var failures = await colourUtils.AnalyzeSiteContrastAsync(
                "https://example.com",
                "<html><body><p>Readable text</p></body></html>");

            Assert.Empty(failures);
        }

        [Fact]
        public async Task RelativeFontSizeDoesNotCrashContrastAnalysis()
        {
            var failures = await colourUtils.AnalyzeSiteContrastAsync(
                "https://example.com",
                "<html><head><style>p { font-size: 1rem; color: #000; }</style></head><body><p>Readable text</p></body></html>");

            Assert.Empty(failures);
        }

        [Fact]
        public async Task StylesheetBackgroundIsUsedWhenRelativeFontSizePreventsComputedStyle()
        {
            var failures = await colourUtils.AnalyzeSiteContrastAsync(
                "https://example.com",
                "<html><head><style>.container { background-color: #000; font-size: 1rem; } span { color: #fff; }</style></head><body><div class='container'><span>Readable text</span></div></body></html>");

            Assert.Empty(failures);
        }

            [Fact]
    public async Task Fetcher_ThrowsAnalysisException_OnUnreachableHost()
    {
        var fetcher = new SourceFetcher();
        await Assert.ThrowsAsync<AnalysisException>(() =>
            fetcher.GetHtmlAsync("https://this-domain-should-not-exist-xyz123.com"));
    }
    }
