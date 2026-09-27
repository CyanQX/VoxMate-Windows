namespace VoiceTranslator.Core;

public sealed class AppSettings
{
    public bool AlwaysOnTop { get; set; } = true;
    public int MicrophoneIndex { get; set; }
    public string WhisperExecutablePath { get; set; } = string.Empty;
    public string WhisperModelPath { get; set; } = string.Empty;
    public TranslationMode TranslationMode { get; set; } = TranslationMode.Direct;
    public string SourceLanguageCode { get; set; } = "zh";
    public string TargetLanguageCode { get; set; } = "en";
    public string LlamaExecutablePath { get; set; } = string.Empty;
    public string LlamaModelPath { get; set; } = string.Empty;
    public bool AutoTranslate { get; set; } = true;
    public bool AutoCopyTranslation { get; set; } = true;
    public bool MinimizeToTrayOnClose { get; set; } = true;
    public bool SaveHistory { get; set; }
}
