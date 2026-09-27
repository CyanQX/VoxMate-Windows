namespace VoiceTranslator.Core;

public sealed record TranslationRequest(string SourceText, string TargetLanguage, TranslationMode Mode, string SourceLanguage = "zh");

public sealed record TranslationResult(string Text, string Provider);

public interface ITranslationProvider
{
    Task<TranslationResult> TranslateAsync(TranslationRequest request, AppSettings settings, CancellationToken cancellationToken);
}
