namespace VoiceTranslator.Core;

public enum AssistantState
{
    Idle,
    Starting,
    Recording,
    ProcessingAudio,
    Recognizing,
    SourceReady,
    Translating,
    Completed,
    Error
}
