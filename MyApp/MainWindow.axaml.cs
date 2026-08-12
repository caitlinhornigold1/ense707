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
        string url = WebsiteUrlTextBox.Text ?? "";

        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var fetcher = new SourceFetcher();

        string html = await fetcher.GetHtmlAsync(url);

        var parser = new HtmlParser();

        var document = await parser.ParseAsync(html);

        // Website is now parsed.
        Console.WriteLine("HTML successfully parsed!");
    }
}