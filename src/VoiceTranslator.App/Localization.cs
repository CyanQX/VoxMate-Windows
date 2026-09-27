using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows.Markup;

namespace VoiceTranslator.App;

public static class Localization
{
    private static readonly Lazy<IReadOnlyDictionary<string, string>> Strings = new(Load);

    public static bool IsChinese => Strings.Value.Count != 0;

    public static string T(string english) => Strings.Value.TryGetValue(english, out var translated)
        && !string.IsNullOrWhiteSpace(translated) ? translated : english;

    public static string T(string english, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, T(english), args);

    private static IReadOnlyDictionary<string, string> Load()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "Locales");
        string marker = Path.Combine(directory, "active-locale.txt");
        string pack = Path.Combine(directory, "zh-CN.json");
        try
        {
            if (!File.Exists(marker) || !File.Exists(pack) ||
                !string.Equals(File.ReadAllText(marker).Trim(), "zh-CN", StringComparison.OrdinalIgnoreCase))
                return new Dictionary<string, string>();
            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(pack))
                ?? new Dictionary<string, string>();
        }
        catch (IOException) { return new Dictionary<string, string>(); }
        catch (UnauthorizedAccessException) { return new Dictionary<string, string>(); }
        catch (JsonException) { return new Dictionary<string, string>(); }
    }
}

[MarkupExtensionReturnType(typeof(string))]
public sealed class TrExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;
    public override object ProvideValue(IServiceProvider serviceProvider) => Localization.T(Key);
}

[MarkupExtensionReturnType(typeof(string))]
public sealed class CurrentLanguageExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) =>
        Localization.IsChinese ? Localization.T("Simplified Chinese") : "English";
}
