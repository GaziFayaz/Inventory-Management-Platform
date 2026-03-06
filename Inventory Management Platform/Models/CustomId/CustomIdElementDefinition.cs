namespace Inventory_Management_Platform.Models.CustomId;

public sealed class CustomIdElementDefinition
{
    public CustomIdElementType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool HasPattern { get; init; }
    public string? PatternPlaceholder { get; init; }
    public string? PatternRegex { get; init; }
    public string? PatternErrorMessage { get; init; }
    public IReadOnlyList<CustomIdPatternExample>? PatternExamples { get; init; }
}
