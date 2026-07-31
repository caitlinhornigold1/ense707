namespace ColourContrast
{
    public class ContrastCheck
    {
        public static Boolean textVsBackground(string text, string background)
        {
         
            if (text. && background)
            {
                string trimmedText = text.Trim();
                string trimmedBackground = background.Trim();
                var parts = trimmedText.Replace("rgba", "")
                           .Replace("rgb", "")
                           .Replace("(", "")4
                           .Replace(")", "")
                           .Split(',');
            }
        }
    }
}
