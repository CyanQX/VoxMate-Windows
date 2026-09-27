using System.Windows;
using System.Windows.Controls;
using VoiceTranslator.Core;

namespace VoiceTranslator.App;

public partial class HistoryWindow : Window
{
    private readonly MainViewModel _viewModel;

    public HistoryWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        Reload();
    }

    private void FilterChanged(object sender, RoutedEventArgs e)
    {
        if (_viewModel is not null) Reload();
    }

    private void Reload()
    {
        string query = SearchBox?.Text.Trim() ?? string.Empty;
        string mode = (ModeFilter?.SelectedItem as ComboBoxItem)?.Tag as string ?? "All";
        var entries = _viewModel.History.Load()
            .Where(item => (mode == "All" || item.Mode.ToString() == mode) &&
                           (query.Length == 0 || item.SourceText.Contains(query, StringComparison.OrdinalIgnoreCase) || item.TranslationText.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(item => item.CreatedAt).ToList();
        EntriesList.ItemsSource = entries;
        NoticeText.Text = !_viewModel.SaveHistory
            ? Localization.T("History is off. Enable it in Basic Settings; existing entries can still be viewed and deleted.")
            : entries.Count == 0 ? Localization.T("No matching entries.") : Localization.T("{0} entries, stored only on this device.", entries.Count);
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: HistoryEntry item }) Clipboard.SetText(item.TranslationText);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: HistoryEntry item }) { _viewModel.History.Delete(item.Id); Reload(); }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.History.Load().Count == 0) return;
        if (MessageBox.Show(this, Localization.T("Clear all local history entries?"), Localization.T("Clear history"), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        _viewModel.History.Clear();
        Reload();
    }
}
