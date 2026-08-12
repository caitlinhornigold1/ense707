using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using AccessibilityAnalyser.Core;

namespace MyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

private async void AnalyseWebsite_Click(
    object? sender,
    RoutedEventArgs e)
{
    string url = WebsiteUrlTextBox.Text?.Trim() ?? "";

    // Clear previous error
    UrlErrorText.Text = "";

    if (string.IsNullOrWhiteSpace(url))
    {
        UrlErrorText.Text = "Please enter a website URL.";
        return;
    }

    if (!url.StartsWith("http://") && !url.StartsWith("https://"))
    {
        url = "https://" + url;
    }

    if (!IsValidUrl(url))
    {
        UrlErrorText.Text = "Please enter a valid website URL.";
        return;
    }

    try
    {
        var fetcher = new SourceFetcher();

        string html = await fetcher.GetHtmlAsync(url);

        var parser = new HtmlParser();

        var document = await parser.ParseAsync(html);

        Console.WriteLine("HTML successfully parsed!");
    }
    catch (Exception)
    {
        UrlErrorText.Text = "Unable to access this website. Please check the URL.";
        return;
    }
}

private bool IsValidUrl(string url)
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
    {
        return false;
    }

    return uri.Scheme == Uri.UriSchemeHttp ||
           uri.Scheme == Uri.UriSchemeHttps;
}

}