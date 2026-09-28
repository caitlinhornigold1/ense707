using System;

namespace AccessibilityAnalyser.Core;

public static class UrlValidator
{
    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(
            url,
            UriKind.Absolute,
            out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp ||
                uri.Scheme == Uri.UriSchemeHttps);
    }
}