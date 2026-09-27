using System.Windows;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using VoiceTranslator.Core;

namespace VoiceTranslator.App;

public partial class SettingsWindow : Window
{
    private readonly MainViewModel _viewModel;
    public bool MiniModeRequested { get; private set; }

    public SettingsWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        DisplayModeCombo.SelectionChanged += (_, _) =>
        {
            if (DisplayModeCombo.SelectedIndex == 1)
            {
                MiniModeRequested = true;
                Close();
            }
        };
        TranslationModeCombo.SelectedIndex = (int)viewModel.SelectedTranslationMode;
        ShowPanel("basic");
    }

    private void ShowPanel(string panel)
    {
        BasicPanel.Visibility = panel == "basic" ? Visibility.Visible : Visibility.Collapsed;
        SpeechPanel.Visibility = panel == "speech" ? Visibility.Visible : Visibility.Collapsed;
        TranslationPanel.Visibility = panel == "translation" ? Visibility.Visible : Visibility.Collapsed;
        var selected = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(233, 236, 255));
        BasicNav.Background = panel == "basic" ? selected : System.Windows.Media.Brushes.White;
        SpeechNav.Background = panel == "speech" ? selected : System.Windows.Media.Brushes.White;
        TranslationNav.Background = panel == "translation" ? selected : System.Windows.Media.Brushes.White;
    }

    private void BasicNav_Click(object sender, RoutedEventArgs e) => ShowPanel("basic");
    private void SpeechNav_Click(object sender, RoutedEventArgs e) => ShowPanel("speech");
    private void TranslationNav_Click(object sender, RoutedEventArgs e) => ShowPanel("translation");
    private void HistoryNav_Click(object sender, RoutedEventArgs e) => new HistoryWindow(_viewModel) { Owner = this }.ShowDialog();
    private void TranslationModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel is not null && TranslationModeCombo.SelectedItem is ComboBoxItem { Tag: string tag } && Enum.TryParse<TranslationMode>(tag, out var mode))
            _viewModel.SelectedTranslationMode = mode;
    }
    private void RefreshDevices_Click(object sender, RoutedEventArgs e) => _viewModel.RefreshDevices();
    private void BrowseExecutable_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "whisper-cli.exe|whisper-cli.exe|可执行文件|*.exe" };
        if (dialog.ShowDialog(this) == true) _viewModel.WhisperExecutablePath = dialog.FileName;
    }
    private void BrowseModel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Whisper GGML 模型|ggml-*.bin|模型文件|*.bin" };
        if (dialog.ShowDialog(this) == true) _viewModel.WhisperModelPath = dialog.FileName;
    }
    private void BrowseLlamaExecutable_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "llama-cli.exe|llama-cli.exe|可执行文件|*.exe" };
        if (dialog.ShowDialog(this) == true) _viewModel.LlamaExecutablePath = dialog.FileName;
    }
    private void BrowseLlamaModel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "GGUF 模型|*.gguf" };
        if (dialog.ShowDialog(this) == true) _viewModel.LlamaModelPath = dialog.FileName;
    }
    private void OpenModelsFolder_Click(object sender, RoutedEventArgs e)
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VoiceTranslator", "Models");
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo("explorer.exe") { ArgumentList = { path }, UseShellExecute = true });
    }
}
