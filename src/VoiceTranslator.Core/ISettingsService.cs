namespace VoiceTranslator.Core;

public interface ISettingsService
{
    string SettingsPath { get; }
    AppSettings Load();
    void Save(AppSettings settings);
}
