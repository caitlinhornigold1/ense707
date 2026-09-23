using AccessibilityAnalyser.Core;
using Xunit;

namespace AccessibilityAnalyser.Tests;

public class UrlValidatorTests
{
    [Fact]
    public void ValidHttpsUrl_ShouldBeAccepted()
    {
        // Arrange
        string url = "https://example.com";

        // Act
        bool result = UrlValidator.IsValidUrl(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidHttpUrl_ShouldBeAccepted()
    {
        // Arrange
        string url = "http://example.com";

        // Act
        bool result = UrlValidator.IsValidUrl(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InvalidUrl_ShouldBeRejected()
    {
        // Arrange
        string url = "not-a-valid-url";

        // Act
        bool result = UrlValidator.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UrlWithoutHttpScheme_ShouldBeRejected()
    {
        // Arrange
        string url = "example.com";

        // Act
        bool result = UrlValidator.IsValidUrl(url);

        // Assert
        Assert.False(result);
    }
}