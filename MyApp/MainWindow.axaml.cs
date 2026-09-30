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
            $"Automated WCAG 2.2 Assessment: {report.FinalScore:F1}%";

        ScoreDetailsTextBlock.Text =
            $"{report.PassedCriteria} of {report.TestedCriteria} tested Success Criteria passed " +
            $"(Level A: {report.LevelAPassed}/{report.LevelATested}, " +
            $"Level AA: {report.LevelAAPassed}/{report.LevelAATested}).";


        // Alt text

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


        // Keyboard accessibility

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


        // Responsive design

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

// form labels
FormLabelSummaryTextBlock.Text =
    $"Form label issues: {report.FormLabelIssues.Count}";

FormLabelIssuesPanel.Children.Clear();

if (report.FormLabelIssues.Count > 0)
{
    FormLabelExpander.IsVisible = true;

    foreach (var issue in report.FormLabelIssues)
    {
        FormLabelIssuesPanel.Children.Add(new TextBlock
        {
            Text =
                $"• {issue.TagName} " +
                $"(Type: {issue.Type}, ID: {issue.Id}, Name: {issue.Name}) " +
                "does not have an accessible label.",
            FontSize = 15,
            Foreground = Avalonia.Media.Brushes.White,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 5, 0, 5)
        });
    }
}
else
{
    FormLabelExpander.IsVisible = false;
}

// html structure
MalformedHtmlSummaryTextBlock.Text =
    $"HTML structure issues: {report.MalformedHtmlIssues.Count}";

MalformedHtmlIssuesPanel.Children.Clear();

if (report.MalformedHtmlIssues.Count > 0)
{
    MalformedHtmlExpander.IsVisible = true;

    foreach (var issue in report.MalformedHtmlIssues)
    {
        MalformedHtmlIssuesPanel.Children.Add(new TextBlock
        {
            Text = $"• Unclosed <{issue}> element detected.",
            FontSize = 15,
            Foreground = Avalonia.Media.Brushes.White,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 5, 0, 5)
        });
    }
}
else
{
    MalformedHtmlExpander.IsVisible = false;
}

// empty links
EmptyLinkSummaryTextBlock.Text =
    $"Empty link issues: {report.EmptyLinkIssues.Count}";

EmptyLinkIssuesPanel.Children.Clear();

if (report.EmptyLinkIssues.Count > 0)
{
    EmptyLinkExpander.IsVisible = true;

    foreach (var issue in report.EmptyLinkIssues)
    {
        EmptyLinkIssuesPanel.Children.Add(new TextBlock
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
    EmptyLinkExpander.IsVisible = false;
}

// duplicate IDs
DuplicateIdSummaryTextBlock.Text =
    $"Duplicate ID issues: {report.DuplicateIdIssues.Count}";

DuplicateIdIssuesPanel.Children.Clear();

if (report.DuplicateIdIssues.Count > 0)
{
    DuplicateIdExpander.IsVisible = true;

    foreach (var issue in report.DuplicateIdIssues)
    {
        DuplicateIdIssuesPanel.Children.Add(new TextBlock
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
    DuplicateIdExpander.IsVisible = false;
}
//missing lang tag
MissingLanguageSummaryTextBlock.Text =
    $"Missing language issues: {report.MissingLanguageIssues.Count}";

MissingLanguageIssuesPanel.Children.Clear();

if (report.MissingLanguageIssues.Count > 0)
{
    MissingLanguageExpander.IsVisible = true;

    foreach (var issue in report.MissingLanguageIssues)
    {
        MissingLanguageIssuesPanel.Children.Add(new TextBlock
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
    MissingLanguageExpander.IsVisible = false;
}

// missing page title
MissingPageTitleSummaryTextBlock.Text =
    $"Missing page title issues: {report.MissingPageTitleIssues.Count}";

MissingPageTitleIssuesPanel.Children.Clear();

if (report.MissingPageTitleIssues.Count > 0)
{
    MissingPageTitleExpander.IsVisible = true;

    foreach (var issue in report.MissingPageTitleIssues)
    {
        MissingPageTitleIssuesPanel.Children.Add(new TextBlock
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
    MissingPageTitleExpander.IsVisible = false;
}

// button labels
ButtonTextSummaryTextBlock.Text =
    $"Button and link label issues: {report.ButtonTextIssues}";

ButtonTextIssuesPanel.Children.Clear();

if (report.ButtonTextIssueDetails.Count > 0)
{
    ButtonTextExpander.IsVisible = true;

    foreach (var issue in report.ButtonTextIssueDetails)
    {
        ButtonTextIssuesPanel.Children.Add(new TextBlock
        {
            Text =
                $"• <{issue.ElementName}> " +
                (string.IsNullOrWhiteSpace(issue.Id)
                    ? "does not have accessible text or a label."
                    : $"(ID: {issue.Id}) does not have accessible text or a label."),

            FontSize = 15,
            Foreground = Avalonia.Media.Brushes.White,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 5, 0, 5)
        });
    }
}
else
{
    ButtonTextExpander.IsVisible = false;
}
        // colour contrast

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