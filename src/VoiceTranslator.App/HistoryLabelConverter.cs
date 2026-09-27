using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VoiceTranslator.App;

public sealed class HistoryLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string label) return DependencyProperty.UnsetValue;
        string[] languages = label.Split('→', 2);
        return languages.Length == 2
            ? $"{Localization.T(languages[0].Trim())} → {Localization.T(languages[1].Trim())}"
            : Localization.T(label);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
