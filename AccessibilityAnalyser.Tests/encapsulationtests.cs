using System.Collections.Generic;
using System.Threading.Tasks;
using AccessibilityAnalyser.Core;
using Xunit;

namespace AccessibilityAnalyser.Tests;

public class EncapsulationTests
{
    [Fact]
    public async Task UnclosedElement_ReturnsUnclosedDivTag()
    {
        Console.WriteLine("Beep Boop");
        var html = @"
            <html><body>
                <div id='header'>
                <div id='content'></div>
                <div id='header'></div>
                <span id='content'></span>
            </body></html>";

        var analyzer = new malformedHtml();
        var expected = new List<string> { "div" };

        IReadOnlyList<string> result = await analyzer.ScanAsync(html);

        Assert.Equal(expected, result);
    }
}