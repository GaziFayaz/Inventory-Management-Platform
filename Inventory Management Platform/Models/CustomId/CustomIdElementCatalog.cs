namespace Inventory_Management_Platform.Models.CustomId;

public static class CustomIdElementCatalog
{
    public static readonly IReadOnlyList<CustomIdElementDefinition> All =
    [
        new CustomIdElementDefinition
        {
            Type = CustomIdElementType.Fixed,
            Name = "Fixed",
            Description = "A piece of unchanging text. E.g., you can use Unicode emoji.",
            HasPattern = true,
            PatternPlaceholder = "Enter text...",
            PatternRegex = @"^.{1,50}$",
            PatternErrorMessage = "Fixed text must be between 1 and 50 characters.",
            PatternExamples =
            [
                new CustomIdPatternExample { Pattern = "INV-", Produces = "INV-", Label = "Prefix" },
                new CustomIdPatternExample { Pattern = "📚", Produces = "📚", Label = "Emoji" }
            ]
        },
        new CustomIdElementDefinition
        {
            Type = CustomIdElementType.Random20Bit,
            Name = "20-bit random",
            Description = "A random value (0–1,048,575). Format as decimal (D1–D7) or hex (X1–X5).",
            HasPattern = true,
            PatternPlaceholder = "X5",
            PatternRegex = @"^([Dd][1-7]?|[Xx][1-5]?)$",
            PatternErrorMessage = "Use D or D1–D7 for decimal, X or X1–X5 for hex.",
            PatternExamples =
            [
                new CustomIdPatternExample { Pattern = "D6", Produces = "049182", Label = "6-digit decimal" },
                new CustomIdPatternExample { Pattern = "X5", Produces = "A7E3C", Label = "5-digit hex" },
                new CustomIdPatternExample { Pattern = "D", Produces = "49182", Label = "No leading zeros" }
            ]
        },
        new CustomIdElementDefinition
        {
            Type = CustomIdElementType.Random32Bit,
            Name = "32-bit random",
            Description = "A random value (0–4,294,967,295). Format as decimal (D1–D10) or hex (X1–X8).",
            HasPattern = true,
            PatternPlaceholder = "D10",
            PatternRegex = @"^([Dd]([1-9]|10)?|[Xx][1-8]?)$",
            PatternErrorMessage = "Use D or D1–D10 for decimal, X or X1–X8 for hex.",
            PatternExamples =
            [
                new CustomIdPatternExample { Pattern = "D10", Produces = "0491827364", Label = "10-digit decimal" },
                new CustomIdPatternExample { Pattern = "X8", Produces = "1A2B3C4D", Label = "8-digit hex" },
                new CustomIdPatternExample { Pattern = "D", Produces = "491827364", Label = "No leading zeros" }
            ]
        },
        new CustomIdElementDefinition
        {
            Type = CustomIdElementType.Random6Digit,
            Name = "6-digit random",
            Description = "A random 6-digit decimal number (000000–999999). No pattern needed.",
            HasPattern = false,
            PatternExamples =
            [
                new CustomIdPatternExample { Pattern = "", Produces = "049182", Label = "Always 6 digits" }
            ]
        },
        new CustomIdElementDefinition
        {
            Type = CustomIdElementType.Random9Digit,
            Name = "9-digit random",
            Description = "A random 9-digit decimal number (000000000–999999999). No pattern needed.",
            HasPattern = false,
            PatternExamples =
            [
                new CustomIdPatternExample { Pattern = "", Produces = "049182736", Label = "Always 9 digits" }
            ]
        },
        new CustomIdElementDefinition
        {
            Type = CustomIdElementType.Sequence,
            Name = "Sequence",
            Description = "A sequential index (max existing + 1). Format with leading zeros (D4) or without (D).",
            HasPattern = true,
            PatternPlaceholder = "D3",
            PatternRegex = @"^[Dd][1-9]?$",
            PatternErrorMessage = "Use D for no leading zeros, or D1–D9 to set minimum digits.",
            PatternExamples =
            [
                new CustomIdPatternExample { Pattern = "D4", Produces = "0042", Label = "4-digit with leading zeros" },
                new CustomIdPatternExample { Pattern = "D", Produces = "42", Label = "No leading zeros" }
            ]
        },
        new CustomIdElementDefinition
        {
            Type = CustomIdElementType.DateTime,
            Name = "Date/time",
            Description = "The item creation date and time. E.g., abbreviated day of week (ddd).",
            HasPattern = true,
            PatternPlaceholder = "yyyy",
            PatternRegex = @"^[yMdHhmsftzKgG/:.\-_ ]+$",
            PatternErrorMessage = "Use standard .NET date/time format specifiers (e.g. yyyy, MM, dd, HH, mm).",
            PatternExamples =
            [
                new CustomIdPatternExample { Pattern = "yyyy", Produces = "2026", Label = "Year only" },
                new CustomIdPatternExample { Pattern = "yyyyMMdd", Produces = "20260306", Label = "Compact date" },
                new CustomIdPatternExample { Pattern = "ddd", Produces = "Fri", Label = "Abbreviated day" }
            ]
        },
        new CustomIdElementDefinition
        {
            Type = CustomIdElementType.Guid,
            Name = "GUID",
            Description = "A globally unique identifier. No formatting needed.",
            HasPattern = false,
            PatternExamples =
            [
                new CustomIdPatternExample { Pattern = "", Produces = "f3a1b2c4-...", Label = "Full GUID" }
            ]
        }
    ];
}
