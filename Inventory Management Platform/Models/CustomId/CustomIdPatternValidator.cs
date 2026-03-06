using System.Text.RegularExpressions;

namespace Inventory_Management_Platform.Models.CustomId;

public static class CustomIdPatternValidator
{
    /// <summary>
    /// Validates a single element's pattern against its type's convention.
    /// Returns null on success, or an error message string on failure.
    /// </summary>
    public static string? Validate(CustomIdElement element)
    {
        var def = CustomIdElementCatalog.All.FirstOrDefault(d => d.Type == element.Type);

        if (def is null)
            return $"Unknown element type: '{element.Type}'.";

        if (!def.HasPattern)
        {
            if (!string.IsNullOrEmpty(element.Pattern))
                return $"Element type '{def.Name}' does not accept a pattern.";
            return null;
        }

        if (string.IsNullOrEmpty(element.Pattern))
            return $"A pattern is required for element type '{def.Name}'.";

        if (def.PatternRegex is not null && !Regex.IsMatch(element.Pattern, def.PatternRegex))
            return def.PatternErrorMessage ?? $"Pattern '{element.Pattern}' is not valid for type '{def.Name}'.";

        return null;
    }

    /// <summary>
    /// Validates all elements in a format list.
    /// Returns a list of (index, errorMessage) pairs for any invalid elements.
    /// </summary>
    public static IReadOnlyList<(int Index, string Error)> ValidateAll(IEnumerable<CustomIdElement> elements)
    {
        var errors = new List<(int, string)>();
        var index = 0;
        foreach (var element in elements)
        {
            var error = Validate(element);
            if (error is not null)
                errors.Add((index, error));
            index++;
        }
        return errors;
    }
}
