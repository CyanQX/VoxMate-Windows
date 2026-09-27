using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VoiceTranslator.Audio;
using VoiceTranslator.Core;
using VoiceTranslator.Infrastructure;
using VoiceTranslator.Whisper;
using VoiceTranslator.Translation;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace VoiceTranslator.App;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _provider;
    private Forms.NotifyIcon? _tray;
    private Drawing.Icon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _provider = new ServiceCollection()
            .AddSingleton<ISettingsService, JsonSettingsService>()
            .AddSingleton<IAudioRecorder, NAudioRecorder>()
            .AddSingleton<ISpeechRecognitionService, WhisperCliRecognitionService>()
            .AddSingleton<ITranslationProvider, LlamaCliTranslationProvider>()
            .AddSingleton<IHistoryService, JsonHistoryService>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<MainWindow>()
            .BuildServiceProvider();
        var mainWindow = _provider.GetRequiredService<MainWindow>();
        var viewModel = _provider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
        var menu = new Forms.ContextMenuStrip();
        void Add(string label, Action action) => menu.Items.Add(label, null, (_, _) => Dispatcher.InvokeAsync(action));
        Add(Localization.T("Show main window"), mainWindow.ShowMain);
        Add(Localization.T("Mini mode"), mainWindow.EnterMiniMode);
        menu.Items.Add(new Forms.ToolStripSeparator());
        Add(Localization.T("Start / stop recording"), () => viewModel.ToggleRecordingCommand.Execute(null));
        Add(Localization.T("Stop listening"), () => { if (viewModel.IsRecording) viewModel.ToggleRecordingCommand.Execute(null); });
        menu.Items.Add(new Forms.ToolStripSeparator());
        Add(Localization.T("Settings..."), () => { mainWindow.ShowMain(); mainWindow.OpenSettings(); });
        Add(Localization.T("History..."), () => { mainWindow.ShowMain(); mainWindow.OpenHistory(); });
        Add(Localization.T("Check for updates..."), () => MessageBox.Show(mainWindow, Localization.T("No update channel is configured for this release."), Localization.T("Check for updates"), MessageBoxButton.OK, MessageBoxImage.Information));
        Add(Localization.T("About..."), () => MessageBox.Show(mainWindow, Localization.T("VoxMate · Offline voice translator for Windows"), Localization.T("About"), MessageBoxButton.OK, MessageBoxImage.Information));
        menu.Items.Add(new Forms.ToolStripSeparator());
        Add(Localization.T("Exit"), mainWindow.RequestExit);
        using (var iconStream = GetResourceStream(new Uri("pack://application:,,,/Assets/voxmate.ico"))?.Stream
            ?? throw new InvalidOperationException("The application icon resource is missing."))
            _trayIcon = new Drawing.Icon(iconStream);
        _tray = new Forms.NotifyIcon
        {
            Icon = _trayIcon,
            Text = Localization.T("VoxMate · Ready"),
            ContextMenuStrip = menu,
            Visible = true
        };
        _tray.DoubleClick += (_, _) => Dispatcher.InvokeAsync(mainWindow.ShowMain);
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.State) && _tray is not null)
                _tray.Text = viewModel.IsRecording ? Localization.T("VoxMate · Recording") : "VoxMate · " + viewModel.MiniStatusText;
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_tray is not null) { _tray.Visible = false; _tray.Dispose(); }
        _trayIcon?.Dispose();
        _provider?.Dispose();
        base.OnExit(e);
    }

}
