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
        if (!File.Exists(audioPath)) throw new FileNotFoundException("The recording file does not exist.", audioPath);
        if (!File.Exists(executablePath))
            throw new FileNotFoundException("whisper-cli.exe was not found. Select it in Speech Recognition Settings.", executablePath);
        if (!File.Exists(modelPath))
            throw new FileNotFoundException("The multilingual Whisper model was not found. Select it in Speech Recognition Settings.", modelPath);
        if (!LanguageCatalog.IsSupported(languageCode))
            throw new ArgumentOutOfRangeException(nameof(languageCode), "Unsupported recognition language.");
        if (languageCode != "en" && Path.GetFileName(modelPath).Contains(".en.", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The English-only model cannot recognize the selected language. Choose a multilingual model.");

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
            if (!process.Start()) throw new InvalidOperationException("Could not start whisper.cpp.");
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
                throw new InvalidOperationException($"whisper.cpp recognition failed (exit code {process.ExitCode}): {diagnostic.Trim()}");
            string text = File.Exists(transcriptPath)
                ? (await File.ReadAllTextAsync(transcriptPath, Encoding.UTF8, cancellationToken)).Trim()
                : (await stdout).Trim();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("The transcript is empty. Move closer to the microphone and try again.");
            return languageCode == "zh" ? ChineseTextNormalizer.ToSimplified(text) : text;
        }
        finally
        {
            if (File.Exists(transcriptPath)) File.Delete(transcriptPath);
        }
    }
}
