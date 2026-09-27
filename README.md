# VoxMate

**许可证：** [MIT](LICENSE)。**Code signing policy：** [代码签名政策](docs/code-signing-policy.md)。当前提供的安装器尚未经过受信任的代码签名；项目计划申请 SignPath Foundation 的免费开源签名，能否获批取决于其审核。获批后的签名致谢措辞为 “Free code signing provided by SignPath.io, certificate by SignPath Foundation”。

[SignPath 签名接入说明](docs/signpath-setup.md)列出了获批后的构建和验证步骤；目前工作流默认只生成未签名安装器。

本仓库的 `main` 只保存可构建源码、界面资源和文档。EXE、DLL、模型、本地下载目录与构建产物均不提交。GitHub Actions 的 [源码验证](.github/workflows/verify.yml)在每次推送时运行；[安装器构建](.github/workflows/package.yml)只在维护者手动触发时运行，并将未签名安装器作为短期 Actions 构建产物保存，不会自动发布到 GitHub Release。

Windows 安装包与 SHA-256 校验文件见 [v0.1.0 Release](https://github.com/CyanQX/VoxMate-Windows/releases/tag/v0.1.0)。这个初始安装包未经代码签名，下载后请核对校验值；发行说明见 [版本说明](docs/release-notes-0.1.0.md)。

Windows 11 x64 本地语音翻译助手。点击麦克风或按 `Alt + Space` 开始和停止录音。主窗口可选择中文、英语、法语、日语、俄语作为识别语言和翻译目标，并可一键交换方向。程序使用本机 whisper.cpp 与 llama.cpp；原文和译文都可以编辑、复制，原文也可从剪贴板粘贴。默认自动翻译和自动复制译文，不会自动粘贴或发送 Enter。

## 运行

需要 Windows 11 x64、.NET 10 SDK 和麦克风。在 PowerShell 中执行：

```powershell
./setup-whisper.ps1
./setup-translation.ps1
dotnet run --project src/VoiceTranslator.App -c Release
```

两个安装脚本下载本地运行时与模型，使用固定 SHA-256 校验。whisper.cpp 二进制来自[官方 v1.9.2 发布页](https://github.com/ggml-org/whisper.cpp/releases/tag/v1.9.2)，多语言模型来自 [whisper.cpp 模型仓库](https://huggingface.co/ggerganov/whisper.cpp)。llama.cpp 二进制来自[官方 b11195 发布页](https://github.com/ggml-org/llama.cpp/releases/tag/b11195)；翻译模型是 [Qwen 官方 Qwen2.5 0.5B Instruct GGUF](https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF)。翻译模型下载使用镜像地址，但校验值对应官方模型文件。

普通设置位于 `%LOCALAPPDATA%\VoiceTranslator\settings.json`，模型位于 `%LOCALAPPDATA%\VoiceTranslator\Models\`。录音和转录临时文件使用后删除。本地翻译不会上传文字。历史记录默认关闭；启用后保存在 `%LOCALAPPDATA%\VoiceTranslator\history.json`，可搜索、复制、逐条删除或清空。

## 功能

- 标准主窗口、同步的置顶开关、迷你悬浮窗口、录音和处理状态。
- 16 kHz 单声道 WAV 录音；识别启动在后台执行，设备无响应时提示超时，不阻塞窗口。
- 五种识别及目标语言、双向交换；本地翻译提供直接翻译／AI Prompt／技术开发／学术／商务模式模板。
- 文本卡片内复制、粘贴及翻译操作；顶部清空按钮。编辑原文会清除过期译文。
- 基础设置、语音识别设置、翻译设置、麦克风和模型文件选择；主窗口与设置中的置顶状态同步。
- 历史记录（默认关闭）、系统托盘菜单、`Alt + Space` 全局快捷键（若系统未占用）、翻译完成提示。
- 关闭主窗口时默认缩至托盘；在托盘菜单选择「退出」可完整关闭。

在线 API、GPU 推理、语音自动结束、自动粘贴、更新服务目前未接入；对应控件已明确禁用或标注。当前默认的 0.5B 轻量模型适合快速离线体验，日语、俄语等方向的措辞质量可能有限。可在翻译设置中选择其它兼容 GGUF 模型。

## 构建、测试与发布

```powershell
./build.ps1
./publish.ps1
./publish.ps1 -BundleModels
./publish.ps1 -BundleModels -OutputDirectory artifacts/publish/win-x64-new
```

`build.ps1` 执行 restore、Release build 与测试。`publish.ps1` 生成 Windows x64 自包含 .NET 程序到 `artifacts/publish/win-x64/`，并附上两个模型安装脚本。加 `-BundleModels` 会将本机已下载的运行时和模型打包进去，发布目录复制到另一台 Windows 11 x64 设备后即可离线使用。首次下载和捆绑模型约需 650 MB 额外磁盘空间。

### 生成单文件安装器

先按「运行」一节下载并校验本地运行时与模型，再准备 [Inno Setup 7](https://jrsoftware.org/isdl.php) 的 `ISCC.exe`。构建脚本会优先查找 `.local/tools/InnoSetup7/ISCC.exe`，然后查找系统安装目录或 `PATH`。运行：

```powershell
./build-installer.ps1
```

脚本重新构建、测试并发布含模型的 Windows x64 自包含程序，生成以下文件：

- `artifacts/installer/VoxMate-Setup-0.1.0-win-x64.exe`：给最终用户下载的独立安装器。
- `artifacts/installer/VoxMate-Setup-0.1.0-win-x64.sha256`：对应的 SHA-256 校验值。
- `artifacts/installer/RELEASE_NOTES.md`：可用于 GitHub Release 的说明。

安装器按当前用户安装至 `%LOCALAPPDATA%\Programs\VoxMate`，无需管理员权限；包含程序图标、开始菜单快捷方式、可选桌面快捷方式以及卸载入口。重复运行更新版安装器会使用同一个应用标识升级现有安装。上传到 GitHub 时，可将 EXE 和 `.sha256` 文件作为 Release 附件，并将 `RELEASE_NOTES.md` 作为说明。安装器目前未经过代码签名；项目构建流程不会自动运行安装程序。隐私说明见 [docs/privacy.md](docs/privacy.md)，第三方许可索引见 [licenses/README.md](licenses/README.md)。

若旧版仍从默认发布目录运行，可用 `-OutputDirectory` 指定新目录，再退出旧版并启动新版。

也可直接执行 `dotnet restore VoiceTranslator.sln`、`dotnet build VoiceTranslator.sln -c Release`、`dotnet test VoiceTranslator.sln -c Release`，以及 `dotnet publish src/VoiceTranslator.App/VoiceTranslator.App.csproj -c Release -r win-x64 --self-contained true`。

界面截图和核对记录见 [docs/design-qa.md](docs/design-qa.md)。
