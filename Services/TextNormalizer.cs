using System.Text.RegularExpressions;

namespace GimnasioApp.Services;

/// <summary>
/// Normalizes user-entered text: trims edges, collapses any run of whitespace
/// (spaces, tabs, newlines) into a single space, and returns empty for
/// whitespace-only input.
/// </summary>
public static class TextNormalizer
{
    public static string Collapse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return Regex.Replace(value.Trim(), @"\s{2,}", " ");
    }
}
