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
        Add("显示主窗口", mainWindow.ShowMain);
        Add("迷你模式", mainWindow.EnterMiniMode);
        menu.Items.Add(new Forms.ToolStripSeparator());
        Add("开始／停止录音", () => viewModel.ToggleRecordingCommand.Execute(null));
        Add("暂停监听", () => { if (viewModel.IsRecording) viewModel.ToggleRecordingCommand.Execute(null); });
        menu.Items.Add(new Forms.ToolStripSeparator());
        Add("设置...", () => { mainWindow.ShowMain(); mainWindow.OpenSettings(); });
        Add("历史记录...", () => { mainWindow.ShowMain(); mainWindow.OpenHistory(); });
        Add("检查更新...", () => MessageBox.Show(mainWindow, "当前版本尚未配置更新通道。", "检查更新", MessageBoxButton.OK, MessageBoxImage.Information));
        Add("关于...", () => MessageBox.Show(mainWindow, "VoxMate · Windows 本地语音翻译助手", "关于", MessageBoxButton.OK, MessageBoxImage.Information));
        menu.Items.Add(new Forms.ToolStripSeparator());
        Add("退出", mainWindow.RequestExit);
        using (var iconStream = GetResourceStream(new Uri("pack://application:,,,/Assets/voxmate.ico"))?.Stream
            ?? throw new InvalidOperationException("应用图标资源缺失。"))
            _trayIcon = new Drawing.Icon(iconStream);
        _tray = new Forms.NotifyIcon
        {
            Icon = _trayIcon,
            Text = "VoxMate · 准备就绪",
            ContextMenuStrip = menu,
            Visible = true
        };
        _tray.DoubleClick += (_, _) => Dispatcher.InvokeAsync(mainWindow.ShowMain);
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.State) && _tray is not null)
                _tray.Text = viewModel.IsRecording ? "VoxMate · 正在录音" : "VoxMate · " + viewModel.MiniStatusText;
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
