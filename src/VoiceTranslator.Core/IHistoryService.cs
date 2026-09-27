namespace VoiceTranslator.Core;

public sealed record HistoryEntry(Guid Id, DateTimeOffset CreatedAt, string SourceText, string TranslationText, TranslationMode Mode)
{
    public string SourceLanguageCode { get; init; } = "zh";
    public string TargetLanguageCode { get; init; } = "en";
    public string LanguagePairLabel => $"{LanguageCatalog.DisplayName(SourceLanguageCode)} → {LanguageCatalog.DisplayName(TargetLanguageCode)}";
    public string ModeLabel => Mode switch
    {
        TranslationMode.AiPrompt => "AI Prompt",
        TranslationMode.Technical => "技术开发",
        TranslationMode.Academic => "学术翻译",
        TranslationMode.Business => "商务翻译",
        TranslationMode.Custom => "自定义模式",
        _ => "直接翻译"
    };
}

public interface IHistoryService
{
    IReadOnlyList<HistoryEntry> Load();
    void Add(HistoryEntry entry);
    void Delete(Guid id);
    void Clear();
}
