# VoxMate

VoxMate is an offline voice transcription and translation assistant for Windows 11 x64. Click the microphone or press `Alt + Space` to start and stop recording. Choose Chinese, English, French, Japanese, or Russian as the recognition and translation languages, and swap the direction with one click. Local whisper.cpp and llama.cpp runtimes process speech and text. Both text cards are editable and copyable; you can also paste source text from the clipboard. Automatic translation and copying of the translation are enabled by default. VoxMate never pastes into another app or presses Enter automatically.

**License:** [MIT](LICENSE). **Code signing policy:** [policy and maintainer roles](docs/code-signing-policy.md). The current installer is unsigned. An application for free SignPath Foundation open source code signing has been submitted and is awaiting review; approval is not guaranteed. If approved, the required acknowledgment will read “Free code signing provided by SignPath.io, certificate by SignPath Foundation.” The [SignPath integration guide](docs/signpath-setup.md) describes the build and verification steps after approval.

The `main` branch contains buildable source code, UI assets, and documentation. EXE and DLL files, models, local downloads, and build outputs are excluded. GitHub Actions [verifies the source](.github/workflows/verify.yml) on every push. Maintainers trigger the [installer workflow](.github/workflows/package.yml) manually; its default output is a short-lived, unsigned Actions artifact and it does not publish a GitHub Release automatically.

Download the Windows installer and its SHA-256 checksum from the [v0.1.0 Release](https://github.com/CyanQX/VoxMate-Windows/releases/tag/v0.1.0). This initial installer is unsigned. Check the checksum before running it, and see the [release notes](docs/release-notes-0.1.0.md) for details.

## Run from source

You need Windows 11 x64, the .NET 10 SDK, and a microphone. Run these commands in PowerShell:

```powershell
./setup-whisper.ps1
./setup-translation.ps1
dotnet run --project src/VoiceTranslator.App -c Release
```

The setup scripts download the local runtimes and models and verify fixed SHA-256 checksums. The whisper.cpp binaries come from its [official v1.9.2 release](https://github.com/ggml-org/whisper.cpp/releases/tag/v1.9.2), and the multilingual model comes from the [whisper.cpp model repository](https://huggingface.co/ggerganov/whisper.cpp). The llama.cpp binaries come from its [official b11195 release](https://github.com/ggml-org/llama.cpp/releases/tag/b11195). The translation model is the [official Qwen2.5 0.5B Instruct GGUF](https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF). The translation model download uses a mirror, but its checksum matches the official model file.

Settings are stored at `%LOCALAPPDATA%\VoiceTranslator\settings.json` and models at `%LOCALAPPDATA%\VoiceTranslator\Models\`. Temporary recording and transcription files are removed after use. Offline translation does not upload text. History is off by default; when enabled, it is saved at `%LOCALAPPDATA%\VoiceTranslator\history.json` and can be searched, copied, deleted entry by entry, or cleared.

## Features

- Main window, synchronized always-on-top setting, floating mini window, and recording and processing states.
- 16 kHz mono WAV recording. Recognition starts in the background; an unresponsive microphone produces a timeout message without freezing the window.
- Five recognition and target languages with a swap control. Local translation offers Direct, AI Prompt, Technical, Academic, and Business prompt templates.
- Copy, paste, and translate controls in the text cards, plus a Clear button in the header. Editing source text clears an outdated translation.
- Basic, speech recognition, and translation settings, including microphone and model file selection. The always-on-top setting stays in sync with the main window.
- Optional history, a system tray menu, an `Alt + Space` global shortcut when available, and a translation completion notice.
- Closing the main window minimizes it to the tray by default. Choose Exit from the tray menu to quit fully.

Online APIs, GPU inference, automatic end-of-speech detection, automatic pasting, and update delivery are not implemented. Their controls are disabled or labeled accordingly. The default lightweight 0.5B model is intended for a quick offline experience; translations involving Japanese or Russian may be less fluent. You can select another compatible GGUF model in Translation Settings.

## Build, test, and publish

```powershell
./build.ps1
./publish.ps1
./publish.ps1 -BundleModels
./publish.ps1 -BundleModels -OutputDirectory artifacts/publish/win-x64-new
```

`build.ps1` restores dependencies, builds the Release configuration, and runs tests. `publish.ps1` creates a self-contained Windows x64 .NET application in `artifacts/publish/win-x64/` and includes both model setup scripts. Add `-BundleModels` to include the locally downloaded runtimes and models. The resulting directory can run offline on another Windows 11 x64 machine. The initial downloads and bundled models require approximately 650 MB of additional disk space.

### Build a standalone installer

Download and verify the local runtimes and models as described above, then make the [Inno Setup 7](https://jrsoftware.org/isdl.php) compiler `ISCC.exe` available. The build script checks `.local/tools/InnoSetup7/ISCC.exe`, standard installation directories, and then `PATH`.

```powershell
./build-installer.ps1
```

The script rebuilds, tests, and publishes the self-contained Windows x64 app with bundled models, then creates:

- `artifacts/installer/VoxMate-Setup-0.1.0-win-x64.exe`: standalone installer for users.
- `artifacts/installer/VoxMate-Setup-0.1.0-win-x64.sha256`: SHA-256 checksum for that installer.
- `artifacts/installer/RELEASE_NOTES.md`: text suitable for a GitHub Release.

The installer installs per user to `%LOCALAPPDATA%\Programs\VoxMate` without administrator rights. It includes the app icon, Start menu shortcut, optional desktop shortcut, and an uninstall entry. A newer installer uses the same application ID to upgrade an existing installation. Attach the EXE and `.sha256` file to a GitHub Release and use `RELEASE_NOTES.md` as its description. The current installer is unsigned, and the build process never runs or installs it. See the [privacy policy](docs/privacy.md) and [third-party license index](licenses/README.md).

If an older app instance still uses the default publish directory, specify another directory with `-OutputDirectory`, exit the old app, and then start the new build.

You can also run `dotnet restore VoiceTranslator.sln`, `dotnet build VoiceTranslator.sln -c Release`, `dotnet test VoiceTranslator.sln -c Release`, and `dotnet publish src/VoiceTranslator.App/VoiceTranslator.App.csproj -c Release -r win-x64 --self-contained true` directly.

See the [UI screenshots and verification notes](docs/design-qa.md).
