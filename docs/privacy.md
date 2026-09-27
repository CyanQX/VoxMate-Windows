# VoxMate 隐私说明

VoxMate 的默认语音识别和翻译在本机完成。程序将麦克风录音交给随包的 whisper.cpp 模型识别，再将原文交给随包的 llama.cpp / Qwen 模型翻译；这些功能不要求账号，也不会主动将录音或文本上传到 VoxMate 服务。

录音、识别结果和翻译输出会临时写入 Windows 临时目录中的 `VoiceTranslator` 子目录，正常处理完成或取消后由程序删除。若程序或系统异常退出，残留临时文件可能保留；下次录音会清理超过一天的旧录音文件。用户可以在 Windows 临时目录中自行检查。

设置保存在 `%LOCALAPPDATA%\VoiceTranslator\settings.json`。历史记录默认关闭；用户启用后，识别原文与译文会保存在 `%LOCALAPPDATA%\VoiceTranslator\history.json`，并可在应用内逐条删除或清空。关闭历史记录不会自动删除之前保存的历史文件。

默认启用自动复制译文，因此译文可能出现在 Windows 剪贴板及用户启用的剪贴板历史中。用户可在设置中关闭自动复制。VoxMate 不会自动向其他应用粘贴或发送文本。

安装器默认将程序和随包模型放入当前用户的 `%LOCALAPPDATA%\Programs\VoxMate`。卸载程序不会自动删除上述用户设置及历史记录；若需要完全清除数据，请在退出程序后自行删除 `%LOCALAPPDATA%\VoiceTranslator` 目录。

构建者运行 `setup-whisper.ps1` 和 `setup-translation.ps1` 下载模型及运行时时会连接脚本中指定的下载源。发布的完整安装器已捆绑这些文件，最终用户安装和使用默认本地功能时无需再下载模型。
