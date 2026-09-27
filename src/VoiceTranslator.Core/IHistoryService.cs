namespace VoiceTranslator.Core;

public sealed record HistoryEntry(Guid Id, DateTimeOffset CreatedAt, string SourceText, string TranslationText, TranslationMode Mode)
{
    public string SourceLanguageCode { get; init; } = "zh";
    public string TargetLanguageCode { get; init; } = "en";
    public string LanguagePairLabel => $"{LanguageCatalog.DisplayName(SourceLanguageCode)} → {LanguageCatalog.DisplayName(TargetLanguageCode)}";
    public string ModeLabel => Mode switch
    {
        TranslationMode.AiPrompt => "AI Prompt",
        TranslationMode.Technical => "Technical",
        TranslationMode.Academic => "Academic",
        TranslationMode.Business => "Business",
        TranslationMode.Custom => "Custom",
        _ => "Direct"
    };
}

public interface IHistoryService
{
    IReadOnlyList<HistoryEntry> Load();
    void Add(HistoryEntry entry);
    void Delete(Guid id);
    void Clear();
}
