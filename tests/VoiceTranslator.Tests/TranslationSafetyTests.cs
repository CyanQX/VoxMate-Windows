using VoiceTranslator.Core;
using VoiceTranslator.Translation;

namespace VoiceTranslator.Tests;

public sealed class TranslationSafetyTests
{
    [Fact]
    public void DefaultsKeepHistoryOffAndUseOfflineWorkflow()
    {
        var settings = new AppSettings();
        Assert.False(settings.SaveHistory);
        Assert.True(settings.AutoTranslate);
        Assert.Equal(TranslationMode.Direct, settings.TranslationMode);
    }

    [Fact]
    public async Task EmptySourceIsRejectedBeforeLaunchingModel()
    {
        var provider = new LlamaCliTranslationProvider();
        await Assert.ThrowsAsync<ArgumentException>(() => provider.TranslateAsync(
            new TranslationRequest("  ", "English", TranslationMode.Direct),
            new AppSettings(), CancellationToken.None));
    }

    [Fact]
    public async Task MissingLocalModelShowsRecoveryPath()
    {
        var provider = new LlamaCliTranslationProvider();
        var error = await Assert.ThrowsAsync<FileNotFoundException>(() => provider.TranslateAsync(
            new TranslationRequest("\u4F60\u597D", "English", TranslationMode.Direct),
            new AppSettings { LlamaExecutablePath = "missing-llama-cli.exe", LlamaModelPath = "missing.gguf" },
            CancellationToken.None));
        Assert.Contains("setup-translation.ps1", error.Message);
    }
}
