namespace VoiceTranslator.App;

public sealed record LanguageChoice(string Code, string DisplayName, string ShortName, string FlagFile)
{
    public string FlagUri => $"pack://application:,,,/VoiceTranslator.App;component/Assets/{FlagFile}.png";
}
