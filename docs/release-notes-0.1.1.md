# VoxMate 0.1.1

This update provides an English default interface, a portable Simplified Chinese language pack, and separate English and Chinese Windows installers. Both editions retain the same offline speech recognition and translation features. The interface language is independent of the five selectable speech and translation languages.

## Build artifacts

- `VoxMate-Setup-0.1.1-win-x64.exe`: English installer.
- `VoxMate-Setup-0.1.1-win-x64-zh-CN.exe`: Simplified Chinese installer with the language pack included.
- `VoxMate-zh-CN-language-pack.zip`: extract into the VoxMate installation directory and restart to switch an English installation to Chinese.
- Matching SHA-256 files for the installers.

Both installers are self-contained Windows 11 x64 packages with local runtimes and models. They install for the current user without administrator rights. The installers remain unsigned while the SignPath Foundation application is pending; neither installer should be described as signed until a trusted signature is present. See [interface language instructions](localization.md) and the [code signing policy](code-signing-policy.md).
