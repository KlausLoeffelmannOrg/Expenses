using System.Text.Json.Serialization;

namespace Foundry.Local;

public enum TranslationLanguage { English, German, French, Italian, Danish, Spanish }

[JsonConverter(typeof(JsonStringEnumConverter<TranslationKind>))]
public enum TranslationKind { Word, Idiom, Text }

public sealed class TranslationResult
{
    public required TranslationKind Kind { get; init; }
    public required string SourceLanguage { get; init; }
    public required string Translation { get; init; }
    public required List<TranslationTerm> Terms { get; init; }
    public required string? SourceIdiom { get; init; }
    public required string? EquivalentIdiom { get; init; }
    public required string? Explanation { get; init; }

    internal void Validate()
    {
        if (!Enum.IsDefined(Kind) || string.IsNullOrWhiteSpace(SourceLanguage) || string.IsNullOrWhiteSpace(Translation) || Terms is null
            || Kind == TranslationKind.Word && Terms.Count == 0
            || Kind == TranslationKind.Idiom && string.IsNullOrWhiteSpace(SourceIdiom)
            || Terms.Any(t => t is null || string.IsNullOrWhiteSpace(t.Term) || string.IsNullOrWhiteSpace(t.ContextSample)))
            throw new InvalidDataException("The model returned an incomplete translation.");
    }
}

public sealed class TranslationTerm
{
    public required string Term { get; init; }
    public required string ContextSample { get; init; }
}
