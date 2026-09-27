namespace VoiceTranslator.Core;

public interface IAudioRecorder : IDisposable
{
    IReadOnlyList<AudioInputDevice> GetInputDevices();
    bool IsRecording { get; }
    Task StartAsync(int deviceIndex, CancellationToken cancellationToken);
    Task<string> StopAsync(CancellationToken cancellationToken);
    Task CancelAsync();
}
