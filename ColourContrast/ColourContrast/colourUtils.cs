using System;
using System.Globalization;
using System.Text.RegularExpressions;

public class colourUtils
{
    public readonly struct colourRgb
    {
        public double R { get; } // 0.0 to 1.0
        public double G { get; } // 0.0 to 1.0
        public double B { get; } // 0.0 to 1.0
        public double A { get; } // 0.0 to 1.0

        public colourRgb(double r, double g, double b, double a = 1.0)
        {
            R = Math.Clamp(r, 0.0, 1.0);
            G = Math.Clamp(g, 0.0, 1.0);
            B = Math.Clamp(b, 0.0, 1.0);
            A = Math.Clamp(a, 0.0, 1.0);
        }

        /// <summary>
        /// Blends alpha against a white background (default for web/text readability).
        /// </summary>
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

        // Calculate the WCAG 2.x relative luminance (0.0 to 1.0).
        public double GetRelativeLuminance()
        {
            // flatten alpha if semi-transparent
            var opaque = FlattenAgainstWhite();

            // Convert SRGB to linear RGB using WCAG
            double ConvertComponent(double c) =>
                c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);

            double rLinear = ConvertComponent(opaque.R);
            double gLinear = ConvertComponent(opaque.G);
            double bLinear = ConvertComponent(opaque.B);

            // Perceived brightness weights (SRGB / Rec709)
            return 0.2126 * rLinear + 0.7152 * gLinear + 0.0722 * bLinear;
        }
    }

    // Parse Hex (#RGB, #RGBA, #RRGGBB, #RRGGBBAA), rgb(...), and rgba(...) formats into colourRgb.
    public static colourRgb ParseColour(string colourStr)
    {
        if (string.IsNullOrWhiteSpace(colourStr))
            throw new ArgumentException("colour string cannot be empty.");

        colourStr = colourStr.Trim().ToLowerInvariant();

        // Hex
        if (colourStr.StartsWith("#"))
        {
            string hex = colourStr.Substring(1);

            // Expand 3 or 4-digit shorthand (#abc -> #aabbcc)
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

        // rgb or rgba
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

    // Compute WCAG Contrast Ratio between colours
    // Returns double between 1.0 (lowest) and 21.0 (highest, black vs white).
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