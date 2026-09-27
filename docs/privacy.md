# VoxMate Privacy Policy

VoxMate processes speech recognition and translation locally by default. The bundled whisper.cpp model transcribes microphone recordings, and the bundled llama.cpp / Qwen model translates the source text. These features require no account and do not upload recordings or text to a VoxMate service.

Recordings, recognition results, and translations are written temporarily to a `VoiceTranslator` subdirectory in the Windows temporary directory. They are deleted after normal processing or cancellation. Temporary files may remain after an app or system crash; the next recording removes old audio files more than one day old. Users can inspect their Windows temporary directory.

Settings are stored at `%LOCALAPPDATA%\VoiceTranslator\settings.json`. History is disabled by default. When enabled, source and translated text are stored at `%LOCALAPPDATA%\VoiceTranslator\history.json`. Entries can be searched, copied, deleted individually, or cleared in the app. Disabling history does not automatically delete previously saved history.

Automatic copying of translations is enabled by default, so translations may appear in the Windows clipboard and, if enabled, clipboard history. Users can disable automatic copying in Settings. VoxMate does not paste into other apps or send text automatically.

The installer places the app and bundled models in the current user's `%LOCALAPPDATA%\Programs\VoxMate` directory by default. Uninstalling does not automatically remove user settings or history. To remove that data, exit the app and delete `%LOCALAPPDATA%\VoiceTranslator` manually.

When a developer runs `setup-whisper.ps1` or `setup-translation.ps1`, the scripts connect to the download sources specified in them. The full installer already bundles these runtimes and models, so end users do not need to download models to install or use the default offline features.
