# VoxMate 第三方许可

安装包内包含下列本地运行时、模型和 .NET 依赖。各组件的原始许可文本保存在本目录；组件的版权归各自权利人所有。此索引仅便于查找，不替代许可原文。

| 组件 | 用途 | 许可文件 | 上游项目 |
| --- | --- | --- | --- |
| whisper.cpp v1.9.2 | 本地语音识别运行时 | `whisper-cpp-LICENSE.txt` | https://github.com/ggml-org/whisper.cpp |
| OpenAI Whisper / ggml-base 多语言模型 | 本地语音识别模型 | `openai-whisper-LICENSE.txt` | https://github.com/openai/whisper |
| llama.cpp b11195 | 本地翻译模型运行时 | `llama-cpp-LICENSE.txt` | https://github.com/ggml-org/llama.cpp |
| Qwen2.5-0.5B-Instruct-GGUF | 本地翻译模型 | `qwen-gguf-LICENSE.txt` | https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF |
| NAudio | 麦克风采集 | `naudio-LICENSE.txt` | https://github.com/naudio/NAudio |
| .NET 10 | 自包含应用运行时 | `dotnet-LICENSE.txt`、`dotnet-ThirdPartyNotices.txt` | https://github.com/dotnet/runtime |
| Microsoft.Extensions.DependencyInjection | 依赖注入 | `dependency-injection-ThirdPartyNotices.txt` | https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection |

whisper.cpp 和 llama.cpp 的发布包可能含有额外的第三方声明；其原始目录也随安装包保留。模型及二进制的来源、校验方式见仓库根目录的安装脚本。
