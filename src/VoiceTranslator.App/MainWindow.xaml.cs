using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using VoiceTranslator.Core;
using VoiceTranslator.Platform;

namespace VoiceTranslator.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private bool _closing;
    private bool _exitRequested;
    private MiniWindow? _miniWindow;
    private HwndSource? _windowSource;
    private GlobalHotkeyRegistration? _hotkey;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Topmost = viewModel.AlwaysOnTop;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _windowSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        _hotkey = GlobalHotkeyRegistration.TryRegister(_windowSource.Handle);
        if (_hotkey is not null) _windowSource.AddHook(HotkeyHook);
        _viewModel.HotkeyRegistered = _hotkey is not null;
    }

    private nint HotkeyHook(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == GlobalHotkeyRegistration.Message && wParam == GlobalHotkeyRegistration.Id)
        {
            _viewModel.ToggleRecordingCommand.Execute(null);
            handled = true;
        }
        return 0;
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        if (_hotkey is not null && _windowSource is not null) _windowSource.RemoveHook(HotkeyHook);
        _hotkey?.Dispose();
        _viewModel.HotkeyRegistered = false;
        base.OnClosed(e);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.AlwaysOnTop)) Topmost = _viewModel.AlwaysOnTop;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }
    private void PinButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.AlwaysOnTop = !_viewModel.AlwaysOnTop;
    }
    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    private void TranslationModeButton_Click(object sender, RoutedEventArgs e) => TranslationModePopup.IsOpen = !TranslationModePopup.IsOpen;
    private void ModeOption_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: string tag } && Enum.TryParse<TranslationMode>(tag, out var mode))
            _viewModel.SelectedTranslationMode = mode;
        TranslationModePopup.IsOpen = false;
    }
    private void SettingsButton_Click(object sender, RoutedEventArgs e) => OpenSettings();

    public void OpenSettings()
    {
        var settings = new SettingsWindow(_viewModel) { Owner = this };
        settings.ShowDialog();
        if (settings.MiniModeRequested) EnterMiniMode();
    }

    public void OpenHistory() => new HistoryWindow(_viewModel) { Owner = this }.ShowDialog();

    public void ShowMain()
    {
        if (_miniWindow is not null) { _miniWindow.Close(); return; }
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    public void EnterMiniMode()
    {
        if (_miniWindow is not null) return;
        _miniWindow = new MiniWindow(_viewModel);
        _miniWindow.Closed += (_, _) =>
        {
            _miniWindow = null;
            if (!_exitRequested) { Show(); Activate(); }
        };
        _miniWindow.Show();
        Hide();
    }
    public void RequestExit()
    {
        _exitRequested = true;
        if (_miniWindow is not null)
        {
            _miniWindow.Close();
            _miniWindow = null;
        }
        Close();
    }
    protected override async void OnClosing(CancelEventArgs e)
    {
        if (!_exitRequested && _viewModel.MinimizeToTrayOnClose)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        if (!_closing)
        {
            e.Cancel = true;
            _closing = true;
            try { await _viewModel.ShutdownAsync(); }
            finally { Close(); }
            return;
        }
        base.OnClosing(e);
    }
}
