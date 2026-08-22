using System;

namespace AccessibilityAnalyser.Core;

// A user-facing error for anything that goes wrong during fetching or parsing,
// so callers (Cli, UI) can show a clear message instead of an unhandled crash
public class AnalysisException : Exception
{
    public AnalysisException(string message, Exception inner) : base(message, inner) { }
    public AnalysisException(string message) : base(message) { }
}