using System.Linq;
using System.Threading.Tasks;
using AccessibilityAnalyser.Core;
using AccessibilityAnalyser.altdetect;
using AngleSharp.Css.Dom;
using Xunit;
using AccessibilityAnalyser.Core.Rules;

namespace AccessibilityAnalyser.Tests;
{
    [TestClass]
    public sealed class ColourContrastTests
    {
        [TestMethod]
        public void WhiteTextBlackBackground()
        {
            Double value = colourUtils.GetContrastRatio("#FFFFFF", "#000000");
            Assert.AreEqual(21.0, value);
        }

        [TestMethod]
        public void SameColourBackgroundAndText()
        {
            Double value = colourUtils.GetContrastRatio("#FFFFFF", "#FFFFFF");
            Assert.AreEqual(1.0, value);
        }

        [TestMethod]
        public void ProperContrastButDifferentFormats()
        {
            Double value = colourUtils.GetContrastRatio("#000000", "rgb(255,255,255)");
            Assert.AreEqual(21.0, value);
        }
    }
}
