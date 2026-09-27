# Interface languages

VoxMate 0.1.1 uses English when no language pack is active. Speech recognition and translation can still use Chinese, English, French, Japanese, or Russian independently of the interface language.

## Install the Simplified Chinese pack

Build the pack with `./build-language-pack.ps1`, or build both installers and the pack with `./build-installer.ps1 -Both`. Extract `artifacts/language-packs/VoxMate-zh-CN-language-pack.zip` directly into the installed VoxMate directory, normally `%LOCALAPPDATA%\Programs\VoxMate`. The resulting files must be:

```text
VoxMate/
  VoiceTranslator.App.exe
  Locales/
    active-locale.txt
    zh-CN.json
```

Close VoxMate completely from its tray menu, then start it again. The pack takes effect at startup. To return to English, remove `Locales\active-locale.txt` and restart. The Chinese installer includes these files; the English installer removes the activation marker.

The language pack contains only UTF-8 text. A missing or malformed pack falls back to English. Neither the ZIP nor either installer needs to be installed on the build computer to create the artifacts. The installers are unsigned while the SignPath Foundation application is pending.
