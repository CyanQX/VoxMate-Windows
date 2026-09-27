# VoxMate third-party licenses

The installer bundles the following local runtimes, models, and .NET dependencies. Their original license texts are stored in this directory. Copyright remains with each respective owner. This index helps locate the licenses and does not replace their full text.

| Component | Purpose | License file | Upstream project |
| --- | --- | --- | --- |
| whisper.cpp v1.9.2 | Offline speech recognition runtime | `whisper-cpp-LICENSE.txt` | https://github.com/ggml-org/whisper.cpp |
| OpenAI Whisper / multilingual ggml-base model | Offline speech recognition model | `openai-whisper-LICENSE.txt` | https://github.com/openai/whisper |
| llama.cpp b11195 | Offline translation model runtime | `llama-cpp-LICENSE.txt` | https://github.com/ggml-org/llama.cpp |
| Qwen2.5-0.5B-Instruct-GGUF | Offline translation model | `qwen-gguf-LICENSE.txt` | https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF |
| NAudio | Microphone capture | `naudio-LICENSE.txt` | https://github.com/naudio/NAudio |
| .NET 10 | Self-contained application runtime | `dotnet-LICENSE.txt`, `dotnet-ThirdPartyNotices.txt` | https://github.com/dotnet/runtime |
| Microsoft.Extensions.DependencyInjection | Dependency injection | `dependency-injection-ThirdPartyNotices.txt` | https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection |

The whisper.cpp and llama.cpp release archives may contain additional third-party notices; their original directories are preserved in the installer. See the setup scripts at the repository root for binary and model sources and checksum verification.
