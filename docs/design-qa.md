# UI-01～UI-10 设计与功能核对

参考用户提供的 10 张截图，在 Windows 11 WPF 中实现。以下图片由 WPF 控件实际渲染，核对时使用示例文字和示例历史数据；应用默认不保存历史。

| 参考界面 | 实现截图 | 当前行为 |
|---|---|---|
| UI-01 主窗口 | [完成状态主窗口](images/main-complete.png) | 语言选择与交换、麦克风、双文本卡片及卡片内操作；顶部清空按钮。 |
| UI-02 迷你模式 | [迷你窗口](images/mini-window.png)、[完成状态](images/mini-complete.png) | 悬浮窗口共享主窗口状态和文本，可复制并返回标准模式。 |
| UI-03 录音状态 | [迷你完成状态](images/mini-complete.png) | 启动、录音、识别、翻译、完成和错误状态驱动颜色与文案。设备启动移至后台，超时给出提示。 |
| UI-04 翻译模式 | [主窗口](images/main-complete.png) | 六个模式可选择；直接／AI Prompt／技术／学术／商务使用不同的本地提示词，自定义当前沿用直接翻译。 |
| UI-05 基础设置 | [基础设置](images/basic-settings.png) | 置顶、自动翻译、自动复制、历史保存和关闭到托盘可调整。旧截图仅作阶段布局参考。 |
| UI-06 语音识别设置 | [语音设置](images/speech-settings.png) | 麦克风、whisper.cpp 和模型路径可选择。 |
| UI-07 翻译设置 | [翻译设置](images/translation-settings.png) | 离线引擎、模型路径、五种目标语言和翻译模式可用；在线 API 控件禁用并标注状态。 |
| UI-08 历史记录 | [历史窗口](images/history-window.png) | 启用历史后可搜索、按模式过滤、复制、逐条删除和清空；默认关闭。图中记录是核对用示例。 |
| UI-09 托盘菜单 | 系统托盘中的 VoxMate 图标 | 显示主窗口、迷你模式、录音、设置、历史、关于、退出可用；更新入口提示尚未配置。 |
| UI-10 使用场景 | [完成状态主窗口](images/main-complete.png)、[迷你完成状态](images/mini-complete.png) | 可在其它软件上方悬浮；自动复制开启时显示剪贴板提示，用户可自行按 Ctrl + V。 |

## 验证结果

- `dotnet build VoiceTranslator.sln -c Release --no-restore`：零警告、零错误。
- `build.ps1`：Release 构建零警告、零错误，3 项自动化测试通过。
- 使用真实 Qwen2.5 0.5B GGUF 和 `LlamaCliTranslationProvider`，中文 Spring Boot 句子译为 `Could you please check why the Spring Boot project is not starting?`。
- 先前使用本机中文语音合成得到的 16 kHz WAV，经 `WhisperCliRecognitionService` 得到中文识别结果；尚未测试真实麦克风讲话。
- 本地模型已运行中文→英／法／日／俄和英文→中文方向；五个方向均返回文字，日语与俄语质量受 0.5B 模型限制。
- 当前测试环境的两个麦克风设备在底层启动时均超时；窗口保持响应并给出提示，真实设备录音速度仍需在可用麦克风上验证。
- `publish.ps1 -BundleModels -OutputDirectory artifacts/publish/win-x64-20260927`：生成包含 .NET 运行时、本地模型和二进制文件的 Windows x64 发布目录；发布版进程启动检查通过。原发布目录仍被运行中的旧版占用。

## 已知差异

参考图使用更紧凑的设置与历史窗口。本实现保留更大的输入控件和可滚动设置页，以容纳真实文件路径与状态说明。托盘使用 Windows 原生菜单。在线 API、自动粘贴和语音自动结束保持禁用。历史目前使用本机 JSON 文件；若后续记录量增大，可将 `IHistoryService` 的实现替换为 SQLite。
