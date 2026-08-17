using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AccessibilityAnalyser.Core;

public class ContrastFailure // This is what gets returned
{
    public string ElementTag { get; set; } = string.Empty;
    public string TextSnippet { get; set; } = string.Empty;
    public string TextColour { get; set; } = string.Empty;
    public string BackgroundColour { get; set; } = string.Empty;
    public double ContrastRatio { get; set; }
}

public class colourUtils
{
   public static async Task<List<ContrastFailure>> AnalyzeSiteContrastAsync(string url, string htmlString, double minimumRatio = 4.5)
    {
        var failures = new List<ContrastFailure>();

        // headless DOM
        var config = Configuration.Default.WithCss();
        var context = BrowsingContext.New(config);

        // allow AngleSharp to resolve relative links
        var document = await context.OpenAsync(req => req.Content(htmlString).Address(url));
        var window = document.DefaultView;
        if (window is null)
        {
            return failures;
        }

        var body = document.Body;
        if (body is null)
        {
            return failures;
        }

        // get all elements in the body
        var elements = body.Descendents().OfType<IElement>();

        foreach (var element in elements)
        {
            // test elements that contain text
            bool hasDirectText = element.ChildNodes.Any(n => n.NodeType == NodeType.Text && !string.IsNullOrWhiteSpace(n.TextContent));
            if (!hasDirectText) continue;

            var style = window.GetComputedStyle(element);
            string textColourStr = style.GetPropertyValue("color");
            string bgColourStr = GetEffectiveBackground(element, window);

            try
            {
                double ratio = GetContrastRatio(textColourStr, bgColourStr);

                if (ratio < minimumRatio)
                {
                    failures.Add(new ContrastFailure
                    {
                        ElementTag = element.LocalName,
                        TextSnippet = element.TextContent.Trim(),
                        TextColour = textColourStr,
                        BackgroundColour = bgColourStr,
                        ContrastRatio = Math.Round(ratio, 2)
                    });
                }
            }
            catch (FormatException)
            {
                // skips element if it contains unparsable CSS
                continue;
            }
        }

        return failures;
    }

    // helper to traverse DOM tree if background is transparent
    private static string GetEffectiveBackground(IElement element, IWindow? window)
    {
        if (window is null)
        {
            return "rgb(255, 255, 255)"; // Default browser
        }

        IElement? current = element;
        while (current != null)
        {
            var style = window.GetComputedStyle(current);
            string bg = style.GetPropertyValue("background-color");

            // angleSharp transparent = rgba(0, 0, 0, 0)
            if (!string.IsNullOrWhiteSpace(bg) && bg != "rgba(0, 0, 0, 0)" && bg != "transparent")
            {
                return bg;
            }
            current = current.ParentElement;
        }
        return "rgb(255, 255, 255)"; // Default browser
    }

    public readonly struct colourRgb
    {
        public double R { get; }
        public double G { get; }
        public double B { get; }
        public double A { get; }

        public colourRgb(double r, double g, double b, double a = 1.0)
        {
            R = Math.Clamp(r, 0.0, 1.0);
            G = Math.Clamp(g, 0.0, 1.0);
            B = Math.Clamp(b, 0.0, 1.0);
            A = Math.Clamp(a, 0.0, 1.0);
        }

        public colourRgb FlattenAgainstWhite()
        {
            if (A >= 1.0) return this;
            return new colourRgb(
                R * A + (1.0 - A),
                G * A + (1.0 - A),
                B * A + (1.0 - A),
                1.0
            );
        }

        public double GetRelativeLuminance()
        {
            var opaque = FlattenAgainstWhite();
            double ConvertComponent(double c) =>
                c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);

            double rLinear = ConvertComponent(opaque.R);
            double gLinear = ConvertComponent(opaque.G);
            double bLinear = ConvertComponent(opaque.B);

            return 0.2126 * rLinear + 0.7152 * gLinear + 0.0722 * bLinear;
        }
    }

    public static colourRgb ParseColour(string colourStr)
    {
        if (string.IsNullOrWhiteSpace(colourStr))
            throw new ArgumentException("colour string cannot be empty.");

        colourStr = colourStr.Trim().ToLowerInvariant();

        if (colourStr.StartsWith("#"))
        {
            string hex = colourStr.Substring(1);

            if (hex.Length == 3 || hex.Length == 4)
            {
                string expanded = "";
                foreach (char ch in hex) expanded += $"{ch}{ch}";
                hex = expanded;
            }

            if (hex.Length == 6 || hex.Length == 8)
            {
                byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                byte a = hex.Length == 8 ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) : (byte)255;

                return new colourRgb(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
            }
        }

        var match = Regex.Match(colourStr, @"^rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+)\s*)?\)$");
        if (match.Success)
        {
            double r = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) / 255.0;
            double g = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) / 255.0;
            double b = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture) / 255.0;

            double a = 1.0;
            if (match.Groups[4].Success)
            {
                a = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
            }

            return new colourRgb(r, g, b, a);
        }

        throw new FormatException($"Unsupported colour format: '{colourStr}'");
    }

    public static double GetContrastRatio(string colourA, string colourB)
    {
        colourRgb c1 = ParseColour(colourA);
        colourRgb c2 = ParseColour(colourB);

        double l1 = c1.GetRelativeLuminance();
        double l2 = c2.GetRelativeLuminance();

        double lighter = Math.Max(l1, l2);
        double darker = Math.Min(l1, l2);

        return (lighter + 0.05) / (darker + 0.05);
    }
}