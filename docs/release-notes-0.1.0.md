# VoxMate 0.1.0

VoxMate is an offline voice transcription and translation assistant for Windows 11 x64. It supports choosing and swapping recognition and target languages among Chinese, English, French, Japanese, and Russian, as well as a floating mini window, global shortcut, text copy and paste controls, translation modes, and optional history.

## Download and installation

Download `VoxMate-Setup-0.1.0-win-x64.exe` and follow the installer to install for the current Windows user. Administrator privileges are not required. The installer includes the .NET runtime, speech recognition model, and translation model, and is approximately 643 MiB. Launch VoxMate from the Start menu or choose the optional desktop shortcut during installation. Uninstall it through Windows Installed apps.

`VoxMate-Setup-0.1.0-win-x64.sha256` contains the file's SHA-256 checksum. Verify it in PowerShell:

```powershell
Get-FileHash -Algorithm SHA256 .\VoxMate-Setup-0.1.0-win-x64.exe
```

The installer is unsigned. Windows may display an unknown publisher warning. Download only from the project's official GitHub Release page and verify the checksum before running it.

## Main features

- Click the microphone or press `Alt + Space` to start and stop recording. Recording and recognition run in the background.
- Swap the recognition and target languages. Translation modes include Direct, AI Prompt, Technical, Academic, and Business.
- Edit and copy both source and translated text, and paste clipboard text into the source card. Automatic copying of translations is enabled by default.
- The main window's always-on-top setting stays in sync with Settings. Closing the main window minimizes it to the system tray by default.
- History is disabled by default and can be enabled in Settings.

## Known limitations and data

This release uses a lightweight local translation model. French, Japanese, Russian, and other translations may sound unnatural. Online translation APIs, GPU inference, automatic pasting, and automatic updates are not implemented. For local data processing, settings, and history locations, see the bundled `docs/privacy.md`. Third-party licenses are listed in `licenses/README.md`.

The VoxMate source code is licensed under MIT; see the bundled `LICENSE`. The installer remains unsigned while the project awaits review of its SignPath Foundation open source signing application. It must not be described as signed unless a future release actually receives a trusted signature. If approved, the required acknowledgment will read “Free code signing provided by SignPath.io, certificate by SignPath Foundation.” The [code signing policy](https://github.com/CyanQX/VoxMate-Windows/blob/main/docs/code-signing-policy.md) describes maintainer roles and the signing scope.

The repository `README.md` contains source and build instructions. The installer was not run or installed on the build computer. This initial unsigned release lets users verify its source and SHA-256 checksum themselves.
