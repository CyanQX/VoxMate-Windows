namespace VoiceTranslator.Core;

public static class LanguageCatalog
{
    public static readonly IReadOnlyList<string> Codes = ["zh", "en", "fr", "ja", "ru"];

    public static bool IsSupported(string? code) => code is not null && Codes.Contains(code, StringComparer.OrdinalIgnoreCase);

    public static string EnglishName(string code) => code.ToLowerInvariant() switch
    {
        "zh" => "Chinese",
        "en" or "english" => "English",
        "fr" => "French",
        "ja" => "Japanese",
        "ru" => "Russian",
        _ => throw new ArgumentOutOfRangeException(nameof(code), $"不支持的语言：{code}")
    };

    public static string DisplayName(string code) => code.ToLowerInvariant() switch
    {
        "zh" => "中文",
        "en" or "english" => "English",
        "fr" => "Français",
        "ja" => "日本語",
        "ru" => "Русский",
        _ => code
    };
}
