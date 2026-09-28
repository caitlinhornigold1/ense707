using AccessibilityAnalyser.Core;

namespace AccessibilityAnalyser.Tests;

public class FormLabelDetectionTests
{

    [Fact]
    public async Task ScanAsync_TestLiveSite()
    {
        var fetcher = new SourceFetcher();
		var html = await fetcher.GetHtmlAsync("https://www.midasgroup.online/constrast");

        var missingLabels = await new FormLabelDetection().ScanAsync(html);

        Assert.Single(missingLabels);
        Assert.Contains(missingLabels, element => element.TagName == "input" && element.Id == "fname" && element.Name == "fname");
    }

    [Fact]
    public async Task ScanAsync_ReturnsFormControlsWithoutLabels()
    {
        var html = "<form><input id='email' name='email'><select name='country'></select><textarea name='notes'></textarea></form>";

        var missingLabels = await new FormLabelDetection().ScanAsync(html);

        Assert.Equal(3, missingLabels.Count);
        Assert.Contains(missingLabels, element => element.TagName == "input" && element.Id == "email" && element.Name == "email");
        Assert.Contains(missingLabels, element => element.TagName == "select" && element.Name == "country");
        Assert.Contains(missingLabels, element => element.TagName == "textarea" && element.Name == "notes");
    }

    [Fact]
    public async Task ScanAsync_RecognizesExplicitAndImplicitLabels()
    {
        var html = "<form><label for='email'>Email</label><input id='email'><label>Search<input type='search'></label></form>";

        var missingLabels = await new FormLabelDetection().ScanAsync(html);

        Assert.Empty(missingLabels);
    }

    [Fact]
    public async Task ScanAsync_RecognizesAriaLabelsAndExcludesNonLabelableInputs()
    {
        var html = "<form><input aria-label='Email'><input aria-labelledby='password-label'><span id='password-label'>Password</span><input type='hidden'><input type='submit'><button>Save</button></form>";

        var missingLabels = await new FormLabelDetection().ScanAsync(html);

        Assert.Empty(missingLabels);
    }

    [Fact]
    public async Task ScanAsync_ReportsEmptyLabel()
    {
        var html = "<form><label for='email'> </label><input id='email'></form>";

        var missingLabels = await new FormLabelDetection().ScanAsync(html);

        var missingLabel = Assert.Single(missingLabels);
        Assert.Equal("email", missingLabel.Id);
    }

    [Fact]
    public async Task ScanAsync_UsesAltTextForImageInputs()
    {
        var html = "<form><input type='image' alt='Search'><input type='image' name='missing'></form>";

        var missingLabels = await new FormLabelDetection().ScanAsync(html);

        var missingLabel = Assert.Single(missingLabels);
        Assert.Equal("image", missingLabel.Type);
        Assert.Equal("missing", missingLabel.Name);
    }
}