using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using VoiceTranslator.Core;

namespace VoiceTranslator.App;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IAudioRecorder _recorder;
    private readonly ISpeechRecognitionService _recognizer;
    private readonly ITranslationProvider _translator;
    private readonly IHistoryService _history;
    private readonly ISettingsService _settingsService;
    private readonly AppSettings _settings;
    private AssistantState _state = AssistantState.Idle;
    private string _sourceText = string.Empty;
    private string _translationText = string.Empty;
    private string _successText = string.Empty;
    private string _errorText = string.Empty;
    private AudioInputDevice? _selectedDevice;
    private bool _alwaysOnTop;
    private bool _hotkeyRegistered;
    private LanguageChoice _sourceLanguage;
    private LanguageChoice _targetLanguage;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly DispatcherTimer _successTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private CancellationTokenSource? _recognitionCancellation;
    private Task? _activeRecognitionTask;
    private Task? _activeTranslationTask;
    private CancellationTokenSource? _translationCancellation;

    public MainViewModel(IAudioRecorder recorder, ISpeechRecognitionService recognizer, ITranslationProvider translator, IHistoryService history, ISettingsService settingsService)
    {
        _recorder = recorder;
        _recognizer = recognizer;
        _translator = translator;
        _history = history;
        _settingsService = settingsService;
        _settings = settingsService.Load();
        ResolveBundledAssets();
        _sourceLanguage = AvailableLanguages.FirstOrDefault(language => language.Code == _settings.SourceLanguageCode) ?? AvailableLanguages[0];
        _targetLanguage = AvailableLanguages.FirstOrDefault(language => language.Code == _settings.TargetLanguageCode && language.Code != _sourceLanguage.Code)
            ?? AvailableLanguages.First(language => language.Code != _sourceLanguage.Code);
        _settings.SourceLanguageCode = _sourceLanguage.Code;
        _settings.TargetLanguageCode = _targetLanguage.Code;
        _alwaysOnTop = _settings.AlwaysOnTop;
        _successTimer.Tick += (_, _) => SuccessText = string.Empty;
        ToggleRecordingCommand = new AsyncRelayCommand(ToggleRecordingAsync);
        ClearCommand = new RelayCommand(Clear, () => !IsBusy && State != AssistantState.Recording);
        CopySourceCommand = new RelayCommand(CopySource, () => !string.IsNullOrWhiteSpace(SourceText));
        PasteSourceCommand = new RelayCommand(PasteSource, () => CanChangeLanguages);
        CopyTranslationCommand = new RelayCommand(CopyTranslation, () => !string.IsNullOrWhiteSpace(TranslationText));
        TranslateCommand = new AsyncRelayCommand(TranslateAsync, () => !string.IsNullOrWhiteSpace(SourceText) && CanChangeLanguages);
        SwapLanguagesCommand = new RelayCommand(() => SetLanguagePair(_targetLanguage, _sourceLanguage, true), () => CanChangeLanguages);
        RefreshDevices();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<AudioInputDevice> Devices { get; } = [];
    public AsyncRelayCommand ToggleRecordingCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand CopySourceCommand { get; }
    public RelayCommand PasteSourceCommand { get; }
    public RelayCommand CopyTranslationCommand { get; }
    public AsyncRelayCommand TranslateCommand { get; }
    public RelayCommand SwapLanguagesCommand { get; }
    public IReadOnlyList<LanguageChoice> AvailableLanguages { get; } =
    [
        new("zh", Localization.T("Chinese (Mandarin)"), Localization.T("Chinese"), "cn"),
        new("en", Localization.T("English"), Localization.T("English"), "us"),
        new("fr", Localization.T("French"), Localization.T("French"), "fr"),
        new("ja", Localization.T("Japanese"), Localization.T("Japanese"), "jp"),
        new("ru", Localization.T("Russian"), Localization.T("Russian"), "ru")
    ];

    public AssistantState State
    {
        get => _state;
        private set
        {
            if (_state == value) return;
            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(MiniStatusText));
            OnPropertyChanged(nameof(MiniStatusHint));
            OnPropertyChanged(nameof(RecordingButtonText));
            OnPropertyChanged(nameof(IsRecording));
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(CanChangeLanguages));
            ToggleRecordingCommand.RaiseCanExecuteChanged();
            ClearCommand.RaiseCanExecuteChanged();
            TranslateCommand.RaiseCanExecuteChanged();
            PasteSourceCommand.RaiseCanExecuteChanged();
            SwapLanguagesCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusText => Localization.T(State switch
    {
        AssistantState.Starting => "Connecting to microphone...",
        AssistantState.Recording => "Recording...",
        AssistantState.ProcessingAudio => "Processing audio...",
        AssistantState.Recognizing => "Recognizing speech...",
        AssistantState.SourceReady => "Recognition complete",
        AssistantState.Translating => "Translating...",
        AssistantState.Completed => "Done, ready to copy",
        AssistantState.Error => "Something went wrong",
        _ => "Idle"
    });
    public string RecordingButtonText => Localization.T(State == AssistantState.Starting ? "Starting microphone..." : IsRecording ? "Click again or press Alt + Space to stop" :
        HotkeyRegistered ? "Click to record or press Alt + Space to speak" : "Click to record");
    public bool HotkeyRegistered
    {
        get => _hotkeyRegistered;
        set { _hotkeyRegistered = value; OnPropertyChanged(); OnPropertyChanged(nameof(RecordingButtonText)); OnPropertyChanged(nameof(MiniStatusHint)); }
    }
    public string MiniStatusText => Localization.T(State switch
    {
        AssistantState.Starting => "Connecting...",
        AssistantState.Recording => "Recording...",
        AssistantState.ProcessingAudio => "Processing audio...",
        AssistantState.Recognizing => "Recognizing speech...",
        AssistantState.SourceReady => "Ready to copy",
        AssistantState.Translating => "Translating...",
        AssistantState.Completed => "Ready to copy",
        AssistantState.Error => "Processing failed",
        _ => "Ready"
    });
    public string MiniStatusHint => Localization.T(State switch
    {
        AssistantState.Starting => "Starting device, please wait",
        AssistantState.Recording => "Click the microphone again to stop",
        AssistantState.Recognizing => "Recognizing locally, please wait",
        AssistantState.SourceReady => "Transcript is ready",
        AssistantState.Translating => "Local model is translating",
        AssistantState.Completed => AutoCopyTranslation ? "Translation copied; press Ctrl + V to paste" : "Click to copy the translation",
        _ => HotkeyRegistered ? "Click the microphone or press Alt + Space" : "Click the microphone to start"
    });
    public bool IsRecording => State == AssistantState.Recording;
    public bool IsBusy => State is AssistantState.Starting or AssistantState.ProcessingAudio or AssistantState.Recognizing or AssistantState.Translating;
    public bool CanChangeLanguages => State is not AssistantState.Starting and not AssistantState.Recording and not AssistantState.ProcessingAudio and not AssistantState.Recognizing and not AssistantState.Translating;

    public LanguageChoice SelectedSourceLanguage
    {
        get => _sourceLanguage;
        set
        {
            if (value is null || value.Code == _sourceLanguage.Code || !CanChangeLanguages) return;
            SetLanguagePair(value, value.Code == _targetLanguage.Code ? _sourceLanguage : _targetLanguage, value.Code == _targetLanguage.Code);
        }
    }

    public LanguageChoice SelectedTargetLanguage
    {
        get => _targetLanguage;
        set
        {
            if (value is null || value.Code == _targetLanguage.Code || !CanChangeLanguages) return;
            SetLanguagePair(value.Code == _sourceLanguage.Code ? _targetLanguage : _sourceLanguage, value, value.Code == _sourceLanguage.Code);
        }
    }

    public string SourceResultHeader => Localization.T("{0} (recognized)", _sourceLanguage.ShortName);
    public string TranslationResultHeader => Localization.T("{0} (translation)", _targetLanguage.ShortName);
    public string LanguagePairLabel => $"{_sourceLanguage.ShortName}  →  {_targetLanguage.ShortName}";
    public string PinButtonTooltip => Localization.T(AlwaysOnTop ? "Turn off always on top" : "Always on top");

    private void SetLanguagePair(LanguageChoice source, LanguageChoice target, bool exchangeContent)
    {
        if (source.Code == target.Code) return;
        bool sourceChanged = source.Code != _sourceLanguage.Code;
        bool targetChanged = target.Code != _targetLanguage.Code;
        if (!sourceChanged && !targetChanged) return;
        string previousSource = SourceText;
        string previousTranslation = TranslationText;
        _sourceLanguage = source;
        _targetLanguage = target;
        _settings.SourceLanguageCode = source.Code;
        _settings.TargetLanguageCode = target.Code;
        _settingsService.Save(_settings);
        OnPropertyChanged(nameof(SelectedSourceLanguage));
        OnPropertyChanged(nameof(SelectedTargetLanguage));
        OnPropertyChanged(nameof(SourceResultHeader));
        OnPropertyChanged(nameof(TranslationResultHeader));
        OnPropertyChanged(nameof(LanguagePairLabel));
        OnPropertyChanged(nameof(MiniSourcePreview));
        OnPropertyChanged(nameof(MiniTranslationPreview));
        if (exchangeContent && !string.IsNullOrWhiteSpace(previousTranslation))
        {
            SourceText = previousTranslation;
            TranslationText = previousSource;
            ErrorText = string.Empty;
            SuccessText = string.Empty;
            State = AssistantState.Completed;
        }
        else if (sourceChanged)
            Clear();
        else if (targetChanged)
        {
            TranslationText = string.Empty;
            SuccessText = string.Empty;
            State = string.IsNullOrWhiteSpace(SourceText) ? AssistantState.Idle : AssistantState.SourceReady;
            if (AutoTranslate && !string.IsNullOrWhiteSpace(SourceText)) TranslateCommand.Execute(null);
        }
    }

    public string SourceText
    {
        get => _sourceText;
        set
        {
            if (_sourceText == value) return;
            _sourceText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SourceCount));
            OnPropertyChanged(nameof(MiniSourcePreview));
            CopySourceCommand.RaiseCanExecuteChanged();
            TranslateCommand.RaiseCanExecuteChanged();
            if (State is not AssistantState.Starting and not AssistantState.Recording and not AssistantState.ProcessingAudio and not AssistantState.Recognizing and not AssistantState.Translating)
            {
                TranslationText = string.Empty;
                SuccessText = string.Empty;
                State = string.IsNullOrWhiteSpace(value) ? AssistantState.Idle : AssistantState.SourceReady;
            }
        }
    }
    public int SourceCount => SourceText.Length;
    public string MiniSourcePreview => string.IsNullOrWhiteSpace(SourceText) ? Localization.T("{0} transcript appears here", _sourceLanguage.ShortName) : SourceText;
    public string TranslationText
    {
        get => _translationText;
        set
        {
            if (_translationText == value) return;
            _translationText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TranslationCount));
            OnPropertyChanged(nameof(MiniTranslationPreview));
            CopyTranslationCommand.RaiseCanExecuteChanged();
        }
    }
    public int TranslationCount => TranslationText.Length;
    public string MiniTranslationPreview => string.IsNullOrWhiteSpace(TranslationText) ? Localization.T("{0} translation appears here", _targetLanguage.ShortName) : TranslationText;
    public string SuccessText
    {
        get => _successText;
        private set
        {
            _successText = value;
            _successTimer.Stop();
            if (!string.IsNullOrWhiteSpace(value)) _successTimer.Start();
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSuccess));
        }
    }
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessText);
    public bool AutoTranslate
    {
        get => _settings.AutoTranslate;
        set { _settings.AutoTranslate = value; _settingsService.Save(_settings); OnPropertyChanged(); }
    }
    public bool AutoCopyTranslation
    {
        get => _settings.AutoCopyTranslation;
        set { _settings.AutoCopyTranslation = value; _settingsService.Save(_settings); OnPropertyChanged(); OnPropertyChanged(nameof(MiniStatusHint)); }
    }
    public bool MinimizeToTrayOnClose
    {
        get => _settings.MinimizeToTrayOnClose;
        set { _settings.MinimizeToTrayOnClose = value; _settingsService.Save(_settings); OnPropertyChanged(); }
    }
    public bool SaveHistory
    {
        get => _settings.SaveHistory;
        set { _settings.SaveHistory = value; _settingsService.Save(_settings); OnPropertyChanged(); }
    }
    public IHistoryService History => _history;

    private void ResolveBundledAssets()
    {
        string root = AppContext.BaseDirectory;
        void UseBundled(ref string current, string relative)
        {
            string bundled = Path.Combine(root, relative);
            if (!File.Exists(current) && File.Exists(bundled)) current = bundled;
        }
        string whisperExe = _settings.WhisperExecutablePath;
        string whisperModel = _settings.WhisperModelPath;
        string llamaExe = _settings.LlamaExecutablePath;
        string llamaModel = _settings.LlamaModelPath;
        UseBundled(ref whisperExe, Path.Combine(".local", "whisper", "whisper-cli.exe"));
        UseBundled(ref whisperModel, Path.Combine("Models", "ggml-base.bin"));
        UseBundled(ref llamaExe, Path.Combine(".local", "llama", "llama-cli.exe"));
        UseBundled(ref llamaModel, Path.Combine("Models", "qwen2.5-0.5b-instruct-q4_k_m.gguf"));
        _settings.WhisperExecutablePath = whisperExe;
        _settings.WhisperModelPath = whisperModel;
        _settings.LlamaExecutablePath = llamaExe;
        _settings.LlamaModelPath = llamaModel;
    }
    public TranslationMode SelectedTranslationMode
    {
        get => _settings.TranslationMode;
        set
        {
            if (_settings.TranslationMode == value) return;
            _settings.TranslationMode = value;
            _settingsService.Save(_settings);
            OnPropertyChanged();
            OnPropertyChanged(nameof(TranslationModeLabel));
            TranslationText = string.Empty;
            SuccessText = string.Empty;
            if (!IsBusy && !string.IsNullOrWhiteSpace(SourceText)) State = AssistantState.SourceReady;
        }
    }
    public string TranslationModeLabel => Localization.T(SelectedTranslationMode switch
    {
        TranslationMode.AiPrompt => "AI Prompt",
        TranslationMode.Technical => "Technical",
        TranslationMode.Academic => "Academic",
        TranslationMode.Business => "Business",
        TranslationMode.Custom => "Custom",
        _ => "Direct"
    });
    public string ErrorText
    {
        get => _errorText;
        private set { _errorText = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(ErrorText);

    public AudioInputDevice? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            OnPropertyChanged();
            if (value is not null)
            {
                _settings.MicrophoneIndex = value.Index;
                _settingsService.Save(_settings);
            }
        }
    }

    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set
        {
            if (_alwaysOnTop == value) return;
            _alwaysOnTop = value;
            _settings.AlwaysOnTop = value;
            _settingsService.Save(_settings);
            OnPropertyChanged();
            OnPropertyChanged(nameof(PinButtonTooltip));
        }
    }

    public string WhisperExecutablePath
    {
        get => _settings.WhisperExecutablePath;
        set { _settings.WhisperExecutablePath = value; _settingsService.Save(_settings); OnPropertyChanged(); }
    }
    public string WhisperModelPath
    {
        get => _settings.WhisperModelPath;
        set { _settings.WhisperModelPath = value; _settingsService.Save(_settings); OnPropertyChanged(); }
    }
    public string LlamaExecutablePath
    {
        get => _settings.LlamaExecutablePath;
        set { _settings.LlamaExecutablePath = value; _settingsService.Save(_settings); OnPropertyChanged(); }
    }
    public string LlamaModelPath
    {
        get => _settings.LlamaModelPath;
        set { _settings.LlamaModelPath = value; _settingsService.Save(_settings); OnPropertyChanged(); }
    }

    public void RefreshDevices()
    {
        Devices.Clear();
        try
        {
            foreach (var device in _recorder.GetInputDevices()) Devices.Add(device);
            SelectedDevice = Devices.FirstOrDefault(d => d.Index == _settings.MicrophoneIndex) ?? Devices.FirstOrDefault();
            if (Devices.Count == 0) ErrorText = Localization.T("No microphone detected. Connect one and click Refresh.");
            else ErrorText = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorText = Localization.T("Could not load microphones: {0}", Localization.T(ex.Message));
        }
    }

    private async Task ToggleRecordingAsync()
    {
        if (State == AssistantState.Recording)
        {
            _activeRecognitionTask = StopAndRecognizeAsync();
            try { await _activeRecognitionTask; }
            finally { _activeRecognitionTask = null; }
            return;
        }
        if (IsBusy || SelectedDevice is null)
        {
            ErrorText = Localization.T(SelectedDevice is null ? "Connect and select a microphone first." : "Wait for recognition to finish.");
            return;
        }
        if (!File.Exists(WhisperExecutablePath) || !File.Exists(WhisperModelPath))
        {
            ErrorText = Localization.T("The whisper.cpp executable or multilingual model is missing. Run setup-whisper.ps1 or choose the files in Speech Recognition Settings.");
            State = AssistantState.Error;
            return;
        }
        try
        {
            ErrorText = string.Empty;
            SuccessText = string.Empty;
            State = AssistantState.Starting;
            await _recorder.StartAsync(SelectedDevice.Index, _shutdown.Token);
            State = AssistantState.Recording;
        }
        catch (Exception ex)
        {
            ErrorText = Localization.T("Could not start recording: {0}", Localization.T(ex.Message));
            State = AssistantState.Error;
        }
    }

    private async Task StopAndRecognizeAsync()
    {
        string? audioPath = null;
        try
        {
            State = AssistantState.ProcessingAudio;
            audioPath = await _recorder.StopAsync(_shutdown.Token);
            State = AssistantState.Recognizing;
            _recognitionCancellation = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token);
            _recognitionCancellation.CancelAfter(TimeSpan.FromMinutes(5));
            string result = await _recognizer.TranscribeAsync(
                audioPath, WhisperExecutablePath, WhisperModelPath, _sourceLanguage.Code, _recognitionCancellation.Token);
            SourceText = result;
            TranslationText = string.Empty;
            ErrorText = string.Empty;
            State = AssistantState.SourceReady;
            if (AutoTranslate) await TranslateAsync();
        }
        catch (Exception ex)
        {
            ErrorText = ex is OperationCanceledException ? Localization.T("Recognition timed out. Try a shorter recording.") : Localization.T(ex.Message);
            State = AssistantState.Error;
        }
        finally
        {
            _recognitionCancellation?.Dispose();
            _recognitionCancellation = null;
            if (audioPath is not null && File.Exists(audioPath)) File.Delete(audioPath);
        }
    }

    public async Task TranslateAsync()
    {
        if (string.IsNullOrWhiteSpace(SourceText) || !CanChangeLanguages) return;
        _translationCancellation?.Cancel();
        _translationCancellation?.Dispose();
        _translationCancellation = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token);
        _translationCancellation.CancelAfter(TimeSpan.FromMinutes(5));
        var cancellation = _translationCancellation;
        State = AssistantState.Translating;
        ErrorText = string.Empty;
        SuccessText = string.Empty;
        try
        {
            string sourceSnapshot = SourceText;
            string previousTranslation = TranslationText;
            TranslationMode modeSnapshot = SelectedTranslationMode;
            string sourceLanguageSnapshot = _sourceLanguage.Code;
            string targetLanguageSnapshot = _targetLanguage.Code;
            _activeTranslationTask = _translator.TranslateAsync(new TranslationRequest(sourceSnapshot, targetLanguageSnapshot, modeSnapshot, sourceLanguageSnapshot), _settings, cancellation.Token);
            var result = await (Task<TranslationResult>)_activeTranslationTask;
            if (SourceText != sourceSnapshot || TranslationText != previousTranslation || SelectedTranslationMode != modeSnapshot)
            {
                ErrorText = Localization.T("The source text or mode changed during translation. Click Translate again.");
                State = AssistantState.SourceReady;
                return;
            }
            TranslationText = result.Text;
            State = AssistantState.Completed;
            if (SaveHistory)
            {
                try
                {
                    _history.Add(new HistoryEntry(Guid.NewGuid(), DateTimeOffset.Now, sourceSnapshot, TranslationText, modeSnapshot)
                    {
                        SourceLanguageCode = sourceLanguageSnapshot,
                        TargetLanguageCode = targetLanguageSnapshot
                    });
                }
                catch (Exception ex) { ErrorText = Localization.T("Translation completed, but history could not be saved: {0}", Localization.T(ex.Message)); }
            }
            if (AutoCopyTranslation)
            {
                try
                {
                    Clipboard.SetText(TranslationText);
                    SuccessText = Localization.T("Translation copied. Press Ctrl + V to paste it into the current field.");
                }
                catch (Exception ex)
                {
                    ErrorText = Localization.T("Translation completed, but automatic copying failed: {0}", Localization.T(ex.Message));
                    SuccessText = Localization.T("Translation completed. You can copy it manually.");
                }
            }
            else SuccessText = Localization.T("Translation completed. You can edit or copy it.");
        }
        catch (Exception ex)
        {
            ErrorText = ex is OperationCanceledException ? Localization.T("Translation timed out or was canceled.") : Localization.T(ex.Message);
            State = AssistantState.Error;
        }
        finally
        {
            _activeTranslationTask = null;
            if (ReferenceEquals(_translationCancellation, cancellation)) _translationCancellation = null;
            cancellation.Dispose();
        }
    }

    private void CopyTranslation()
    {
        try { Clipboard.SetText(TranslationText); ErrorText = string.Empty; SuccessText = Localization.T("Translation copied. Press Ctrl + V to paste it into the current field."); }
        catch (Exception ex) { ErrorText = Localization.T("Copy failed: {0}", Localization.T(ex.Message)); }
    }

    private void CopySource()
    {
        try { Clipboard.SetText(SourceText); ErrorText = string.Empty; SuccessText = Localization.T("Transcript copied to the clipboard."); }
        catch (Exception ex) { ErrorText = Localization.T("Copy failed: {0}", Localization.T(ex.Message)); }
    }

    private void PasteSource()
    {
        try
        {
            if (!Clipboard.ContainsText()) { ErrorText = Localization.T("The clipboard has no text to paste."); return; }
            string text = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text)) { ErrorText = Localization.T("The clipboard text is empty."); return; }
            if (text.Length > 5000) { ErrorText = Localization.T("Clipboard text exceeds 5,000 characters. Shorten it before pasting."); return; }
            SourceText = text;
            ErrorText = string.Empty;
            if (AutoTranslate) TranslateCommand.Execute(null);
        }
        catch (Exception ex) { ErrorText = Localization.T("Paste failed: {0}", Localization.T(ex.Message)); }
    }

    private void Clear()
    {
        SourceText = string.Empty;
        TranslationText = string.Empty;
        ErrorText = string.Empty;
        SuccessText = string.Empty;
        State = AssistantState.Idle;
    }

    public async Task ShutdownAsync()
    {
        _shutdown.Cancel();
        _successTimer.Stop();
        _recognitionCancellation?.Cancel();
        _translationCancellation?.Cancel();
        if (_activeRecognitionTask is not null) await _activeRecognitionTask;
        if (_activeTranslationTask is not null) { try { await _activeTranslationTask; } catch (OperationCanceledException) { } }
        try { await _recorder.CancelAsync().WaitAsync(TimeSpan.FromSeconds(2)); }
        catch (TimeoutException) { /* A disconnected audio driver must not prevent shutdown. */ }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
