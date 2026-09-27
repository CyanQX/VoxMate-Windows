namespace VoiceTranslator.Core;

public interface ISpeechRecognitionService
{
    Task<string> TranscribeAsync(
        string audioPath,
        string executablePath,
        string modelPath,
        string languageCode,
        CancellationToken cancellationToken);
}
