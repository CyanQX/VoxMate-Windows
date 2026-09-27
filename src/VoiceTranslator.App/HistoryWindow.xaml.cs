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
            ? "历史保存已关闭。可在基础设置中启用；已有记录仍可查看和删除。"
            : entries.Count == 0 ? "暂无符合条件的记录。" : $"共 {entries.Count} 条记录，仅保存在本机。";
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
        if (MessageBox.Show(this, "确定清空所有本机历史记录？", "清空历史记录", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        _viewModel.History.Clear();
        Reload();
    }
}
