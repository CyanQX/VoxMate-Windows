# UI-01 to UI-10 design and feature review

The Windows 11 WPF implementation follows the ten reference screenshots supplied by the project owner. The images below were rendered by actual WPF controls using sample text and sample history entries for review. History is disabled by default.

| Reference | Implementation screenshot | Current behavior |
| --- | --- | --- |
| UI-01 Main window | [Completed main window](images/main-complete.png) | Language selection and swapping, microphone, two text cards and their controls, and a header Clear button. |
| UI-02 Mini mode | [Mini window](images/mini-window.png), [completed state](images/mini-complete.png) | The floating window shares state and text with the main window. Text can be copied, and the standard window can be restored. |
| UI-03 Recording states | [Completed mini state](images/mini-complete.png) | Starting, recording, recognizing, translating, completed, and error states drive colors and text. Device startup runs in the background and reports a timeout when needed. |
| UI-04 Translation modes | [Main window](images/main-complete.png) | Six selectable modes. Direct, AI Prompt, Technical, Academic, and Business use different local prompts. Custom currently follows Direct translation. |
| UI-05 Basic settings | [Basic settings](images/basic-settings.png) | Always on top, automatic translation, automatic copying, history, and minimize to tray can be changed. The older reference only guided the initial layout. |
| UI-06 Speech settings | [Speech settings](images/speech-settings.png) | Microphone, whisper.cpp executable, and model path can be selected. |
| UI-07 Translation settings | [Translation settings](images/translation-settings.png) | Offline engine, model path, five target languages, and translation modes are available. Online API controls are disabled and labeled. |
| UI-08 History | [History window](images/history-window.png) | When enabled, history can be searched, filtered by mode, copied, deleted entry by entry, or cleared. The screenshot contains sample review data. |
| UI-09 Tray menu | VoxMate icon in the system tray | Show main window, mini mode, recording, settings, history, About, and Exit are available. Check for updates reports that no update channel is configured. |
| UI-10 Usage scenario | [Completed main window](images/main-complete.png), [completed mini state](images/mini-complete.png) | The mini window can float over another app. When automatic copying is on, a clipboard notice tells the user they can press Ctrl + V. |

## Verification results

- `dotnet build VoiceTranslator.sln -c Release --no-restore`: zero warnings and zero errors.
- `build.ps1`: Release build completed with zero warnings and errors; all three automated tests passed.
- With the actual Qwen2.5 0.5B GGUF model and `LlamaCliTranslationProvider`, a Chinese Spring Boot sentence translated to `Could you please check why the Spring Boot project is not starting?`.
- A previous 16 kHz WAV synthesized from Chinese speech on this machine produced Chinese text through `WhisperCliRecognitionService`. Spoken input from a real microphone has not yet been tested.
- The local model returned text for Chinese to English, French, Japanese, and Russian, and English to Chinese. Japanese and Russian quality is limited by the 0.5B model.
- Both microphone devices in the test environment timed out during low-level startup. The window stayed responsive and displayed an explanation. Recording startup speed still needs testing on a working microphone.
- `publish.ps1 -BundleModels -OutputDirectory artifacts/publish/win-x64-20260927` produced a Windows x64 directory containing the .NET runtime, local models, and binaries. A process startup check passed. The earlier default publish directory remained in use by an older running build.

## Known differences

The reference images show more compact Settings and History windows. This implementation uses larger inputs and a scrollable Settings page to accommodate real file paths and status information. The tray uses a native Windows menu. Online APIs, automatic pasting, and automatic end-of-speech detection remain disabled. History currently uses a local JSON file; if it grows substantially, the `IHistoryService` implementation could be replaced with SQLite.
