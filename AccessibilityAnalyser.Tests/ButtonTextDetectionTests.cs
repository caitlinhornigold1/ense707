using AccessibilityAnalyser.Core;

namespace AccessibilityAnalyser.Tests;

public class ButtonTextDetectionTests
{
    [Fact]
    public async Task ScanAsync_CountsEmptyAnchorsAndButtons()
    {
        var html = "<a id='empty-link' href='/next'></a><button id='empty-button'></button><a href='/space'>  </a><button> \n </button>";

        var result = await new ButtonTextDetection().ScanAsync(html);

        Assert.Equal(4, result.Count);
        Assert.Equal(4, result.FailedElements.Count);
        Assert.Collection(result.FailedElements,
            failure => { Assert.Equal("a", failure.ElementName); Assert.Equal("empty-link", failure.Id); },
            failure => { Assert.Equal("button", failure.ElementName); Assert.Equal("empty-button", failure.Id); },
            failure => { Assert.Equal("a", failure.ElementName); Assert.Null(failure.Id); },
            failure => { Assert.Equal("button", failure.ElementName); Assert.Null(failure.Id); });
    }

    [Fact]
    public async Task ScanAsync_IgnoresControlsWithAccessibleTextOrLabels()
    {
        var html = "<a href='/next'>Continue</a><button>Save</button><a aria-label='Search'></a><button title='Close'></button><a><img alt='Home'></a><button aria-labelledby='label'></button><span id='label'>Submit</span>";

        var detector = new ButtonTextDetection();
        var result = await detector.ScanAsync(html);

        Assert.Equal(0, result.Count);
        Assert.Empty(result.FailedElements);
    }

    [Fact]
    public async Task ScanAsync_IgnoresElementsOutsideAnchorAndButton()
    {
        var html = "<div></div><span></span><input type='button'>";

        var result = await new ButtonTextDetection().ScanAsync(html);

        Assert.Equal(0, result.Count);
        Assert.Empty(result.FailedElements);
    }
}