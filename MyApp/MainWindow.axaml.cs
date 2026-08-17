using Avalonia.Controls;
using Avalonia.Interactivity;
using AccessibilityAnalyser.Core;
using System;

namespace MyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RunButton_Click(object? sender, RoutedEventArgs e)
    {
        ErrorTextBlock.IsVisible = false;
        ReportPanel.IsVisible = false;

        var url = WebsiteUrlTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(url))
        {
            ErrorTextBlock.Text = "Please enter a website URL.";
            ErrorTextBlock.IsVisible = true;
            return;
        }

        try
        {
            RunButton.IsEnabled = false;
            RunButton.Content = "Testing...";

            var report = await Report.GenerateReportAsync(url);

            DisplayReport(report);
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = $"Unable to analyse website: {ex}";
            ErrorTextBlock.IsVisible = true;
        }
        finally
        {
            RunButton.IsEnabled = true;
            RunButton.Content = "Run";
        }
    }

    private void DisplayReport(Report report)
    {
        ReportPanel.IsVisible = true;

        WebsiteTextBlock.Text = $"Website: {report.Uri}";

        ScoreTextBlock.Text =
            $"Accessibility Score: {report.FinalScore:F1}";

        AltTextTextBlock.Text =
            $"Missing alt attributes: {report.MissedAltAttributes}";

        ContrastSummaryTextBlock.Text =
            $"Contrast failures: {report.ContrastFailures.Count}";

        ContrastFailuresPanel.Children.Clear();

        if (report.ContrastFailures.Count == 0)
        {
            var noFailures = new TextBlock
            {
                Text = "✓ No colour contrast failures detected.",
                FontSize = 16
            };

            ContrastFailuresPanel.Children.Add(noFailures);
            return;
        }

        foreach (var failure in report.ContrastFailures)
        {
            var failurePanel = new StackPanel
            {
                Spacing = 5
            };

            failurePanel.Children.Add(new TextBlock
            {
                Text = $"Element: {failure.ElementTag}",
                FontWeight = Avalonia.Media.FontWeight.Bold
            });

            failurePanel.Children.Add(new TextBlock
            {
                Text = $"Text: {failure.TextSnippet}",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });

            failurePanel.Children.Add(new TextBlock
            {
                Text = $"Text colour: {failure.TextColour}"
            });

            failurePanel.Children.Add(new TextBlock
            {
                Text = $"Background colour: {failure.BackgroundColour}"
            });

            ContrastFailuresPanel.Children.Add(failurePanel);
        }
    }
}