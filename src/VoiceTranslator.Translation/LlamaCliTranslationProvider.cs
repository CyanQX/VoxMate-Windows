using System.Diagnostics;
using System.Text;
using VoiceTranslator.Core;

namespace VoiceTranslator.Translation;

public sealed class LlamaCliTranslationProvider : ITranslationProvider
{
    public async Task<TranslationResult> TranslateAsync(TranslationRequest request, AppSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SourceText)) throw new ArgumentException("Source text is empty.", nameof(request));
        if (!File.Exists(settings.LlamaExecutablePath) || !File.Exists(settings.LlamaModelPath))
            throw new FileNotFoundException("The local translation runtime or model is missing. Run setup-translation.ps1 or choose the files in Translation Settings.");

        string sourceLanguage = LanguageCatalog.EnglishName(request.SourceLanguage);
        string targetLanguage = LanguageCatalog.EnglishName(request.TargetLanguage);
        if (sourceLanguage == targetLanguage) throw new ArgumentException("Source and target languages must differ.", nameof(request));

        string instruction = request.Mode switch
        {
            TranslationMode.AiPrompt => $"Rewrite this {sourceLanguage} request as a concise, effective {targetLanguage} AI assistant prompt. Preserve every requirement, constraint and code identifier. Output only the prompt.",
            TranslationMode.Technical => $"Translate this {sourceLanguage} text into natural {targetLanguage} for a software development context. Preserve code symbols, paths and technical meaning. Output only the translation.",
            TranslationMode.Academic => $"Translate this {sourceLanguage} text into clear formal academic {targetLanguage}. Preserve meaning and terminology. Output only the translation.",
            TranslationMode.Business => $"Translate this {sourceLanguage} text into professional business {targetLanguage}. Preserve meaning and tone. Output only the translation.",
            _ => $"Translate this {sourceLanguage} text into natural {targetLanguage} faithfully. Preserve meaning, names and code symbols. Output only the translation."
        };
        string targetConstraint = request.TargetLanguage switch
        {
            "ru" => " Write the translation in Russian Cyrillic. Do not answer in English.",
            "ja" => " Write idiomatic Japanese using standard kanji and kana. Translate greetings naturally; do not transliterate Chinese. Do not answer in English.",
            "zh" => " Write the translation in Simplified Chinese characters. Do not answer in English.",
            "fr" => " Write the translation in French. Do not answer in English.",
            _ => string.Empty
        };
        instruction += targetConstraint;
        string outputPath = Path.Combine(Path.GetTempPath(), "VoiceTranslator", $"translation-{Guid.NewGuid():N}.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var start = new ProcessStartInfo(settings.LlamaExecutablePath)
        {
            WorkingDirectory = Path.GetDirectoryName(settings.LlamaExecutablePath)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (string arg in new[] { "-m", settings.LlamaModelPath, "-sys", instruction,
            "-p", request.SourceText, "-st", "-n", "768", "-c", "4096", "-t", "4", "-tb", "4",
            "--temp", "0.1", "--no-display-prompt", "--no-show-timings", "--no-warmup",
            "--log-disable", "--simple-io", "-o", outputPath })
            start.ArgumentList.Add(arg);

        try
        {
            using var process = new Process { StartInfo = start };
            if (!process.Start()) throw new InvalidOperationException("Could not start the local translator.");
            using var registration = cancellationToken.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) { } });
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            await outputTask;
            string error = await errorTask;
            cancellationToken.ThrowIfCancellationRequested();
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Local translation failed: {error.Trim().Split('\n').LastOrDefault()?.Trim() ?? "unknown error"}");
            string transcript = (await File.ReadAllTextAsync(outputPath, Encoding.UTF8, cancellationToken)).Replace("\r\n", "\n", StringComparison.Ordinal);
            const string marker = "\nAssistant:\n";
            int markerIndex = transcript.LastIndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0) throw new InvalidOperationException("The local model returned an unexpected format. Translate again.");
            string translated = transcript[(markerIndex + marker.Length)..].Trim();
            if (string.IsNullOrWhiteSpace(translated)) throw new InvalidOperationException("The local model returned no translation.");
            return new TranslationResult(translated, "llama.cpp / Qwen2.5");
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }
}
