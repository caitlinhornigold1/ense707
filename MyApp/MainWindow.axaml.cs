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

    if (!url.StartsWith("http://") && !url.StartsWith("https://"))
    {
        url = "https://" + url;
    }

    try
    {
        RunButton.IsEnabled = false;
        RunButton.Content = "Testing...";

        var report = await Report.GenerateReportAsync(url);

        DisplayReport(report);
    }
    catch (Exception)
    {
        ErrorTextBlock.Text =
            "Unable to access this website. Please check that the URL is correct and that the website exists.";
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

        // Website
        WebsiteTextBlock.Text =
            $"Website: {report.Uri}";


        // Score
        ScoreTextBlock.Text =
            $"Accessibility Score: {report.FinalScore:F1}";


        // =========================
        // ALT TEXT
        // =========================

        AltTextTextBlock.Text =
            $"Missing alt attributes: {report.MissedAltAttributes}";

        AltTextIssuesPanel.Children.Clear();

        if (report.AltTextIssueDescriptions.Count > 0)
        {
            AltTextExpander.IsVisible = true;

            foreach (var issue in report.AltTextIssueDescriptions)
            {
                AltTextIssuesPanel.Children.Add(new TextBlock
                {
                    Text = $"• {issue}",
                    FontSize = 15,
                    Foreground = Avalonia.Media.Brushes.White,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 5, 0, 5)
                });
            }
        }
        else
        {
            AltTextExpander.IsVisible = false;
        }


        // =========================
        // KEYBOARD ACCESSIBILITY
        // =========================

        KeyboardSummaryTextBlock.Text =
            $"Keyboard accessibility issues: {report.KeyboardIssues}";

        KeyboardIssuesPanel.Children.Clear();

        if (report.KeyboardIssueDescriptions.Count > 0)
        {
            KeyboardExpander.IsVisible = true;

            foreach (var issue in report.KeyboardIssueDescriptions)
            {
                KeyboardIssuesPanel.Children.Add(new TextBlock
                {
                    Text = $"• {issue}",
                    FontSize = 15,
                    Foreground = Avalonia.Media.Brushes.White,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 5, 0, 5)
                });
            }
        }
        else
        {
            KeyboardExpander.IsVisible = false;
        }


        // =========================
        // RESPONSIVE DESIGN
        // =========================

        ResponsiveSummaryTextBlock.Text =
            $"Responsive design issues: {report.ResponsiveIssues}";

        ResponsiveIssuesPanel.Children.Clear();

        if (report.ResponsiveIssueDescriptions.Count > 0)
        {
            ResponsiveExpander.IsVisible = true;

            foreach (var issue in report.ResponsiveIssueDescriptions)
            {
                ResponsiveIssuesPanel.Children.Add(new TextBlock
                {
                    Text = $"• {issue}",
                    FontSize = 15,
                    Foreground = Avalonia.Media.Brushes.White,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 5, 0, 5)
                });
            }
        }
        else
        {
            ResponsiveExpander.IsVisible = false;
        }


        // =========================
        // COLOUR CONTRAST
        // =========================

        ContrastSummaryTextBlock.Text =
            $"Contrast failures: {report.ContrastFailures.Count}";

        ContrastFailuresPanel.Children.Clear();

        if (report.ContrastFailures.Count > 0)
        {
            ContrastExpander.IsVisible = true;

            foreach (var failure in report.ContrastFailures)
            {
                var failurePanel = new StackPanel
                {
                    Spacing = 5
                };

                failurePanel.Children.Add(new TextBlock
                {
                    Text = $"Element: {failure.ElementTag}",
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Foreground = Avalonia.Media.Brushes.Black
                });

                failurePanel.Children.Add(new TextBlock
                {
                    Text = $"Text: {failure.TextSnippet}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Foreground = Avalonia.Media.Brushes.Black
                });

                failurePanel.Children.Add(new TextBlock
                {
                    Text = $"Text colour: {failure.TextColour}",
                    Foreground = Avalonia.Media.Brushes.Black
                });

                failurePanel.Children.Add(new TextBlock
                {
                    Text = $"Background colour: {failure.BackgroundColour}",
                    Foreground = Avalonia.Media.Brushes.Black
                });

                ContrastFailuresPanel.Children.Add(failurePanel);
            }
        }
        else
        {
            ContrastExpander.IsVisible = false;
        }
    }
}