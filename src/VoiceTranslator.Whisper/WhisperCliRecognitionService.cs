using System.Diagnostics;
using System.Text;
using VoiceTranslator.Core;

namespace VoiceTranslator.Whisper;

public sealed class WhisperCliRecognitionService : ISpeechRecognitionService
{
    public async Task<string> TranscribeAsync(
        string audioPath,
        string executablePath,
        string modelPath,
        string languageCode,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(audioPath)) throw new FileNotFoundException("录音文件不存在。", audioPath);
        if (!File.Exists(executablePath))
            throw new FileNotFoundException("找不到 whisper-cli.exe，请在语音识别设置中选择它。", executablePath);
        if (!File.Exists(modelPath))
            throw new FileNotFoundException("找不到多语言 Whisper 模型，请在语音识别设置中选择它。", modelPath);
        if (!LanguageCatalog.IsSupported(languageCode))
            throw new ArgumentOutOfRangeException(nameof(languageCode), "不支持的识别语言。");
        if (languageCode != "en" && Path.GetFileName(modelPath).Contains(".en.", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("英文专用模型无法识别所选语言，请选择多语言模型。");

        string outputPrefix = Path.Combine(Path.GetTempPath(), "VoiceTranslator", $"transcript-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPrefix)!);
        string transcriptPath = outputPrefix + ".txt";
        var startInfo = new ProcessStartInfo(executablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (string argument in new[] { "-m", modelPath, "-f", audioPath, "-l", languageCode, "-otxt", "-of", outputPrefix, "-nt", "-np" })
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        try
        {
            if (!process.Start()) throw new InvalidOperationException("无法启动 whisper.cpp。");
            Task<string> stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> stderr = process.StandardError.ReadToEndAsync(cancellationToken);
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
                throw;
            }
            string diagnostic = await stderr;
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"whisper.cpp 识别失败（退出码 {process.ExitCode}）：{diagnostic.Trim()}");
            string text = File.Exists(transcriptPath)
                ? (await File.ReadAllTextAsync(transcriptPath, Encoding.UTF8, cancellationToken)).Trim()
                : (await stdout).Trim();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("识别结果为空，请靠近麦克风后重试。");
            return languageCode == "zh" ? ChineseTextNormalizer.ToSimplified(text) : text;
        }
        finally
        {
            if (File.Exists(transcriptPath)) File.Delete(transcriptPath);
        }
    }
}
