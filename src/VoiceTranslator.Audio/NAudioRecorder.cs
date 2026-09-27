using NAudio.Wave;
using VoiceTranslator.Core;

namespace VoiceTranslator.Audio;

public sealed class NAudioRecorder : IAudioRecorder
{
    private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(5);
    private readonly object _gate = new();
    private Session? _active;
    private Task<Session>? _pendingStart;

    public NAudioRecorder() => _ = Task.Run(CleanStaleRecordings);

    public bool IsRecording { get { lock (_gate) return _active is not null; } }

    public IReadOnlyList<AudioInputDevice> GetInputDevices()
    {
        var devices = new List<AudioInputDevice>();
        for (int index = 0; index < WaveIn.DeviceCount; index++)
        {
            var capabilities = WaveIn.GetCapabilities(index);
            devices.Add(new AudioInputDevice(index, capabilities.ProductName));
        }
        return devices;
    }

    public async Task StartAsync(int deviceIndex, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (deviceIndex < 0 || deviceIndex >= WaveIn.DeviceCount)
            throw new InvalidOperationException("The selected microphone is unavailable. Select another device.");

        Task<Session> start;
        lock (_gate)
        {
            if (_active is not null) throw new InvalidOperationException("Recording has already started.");
            if (_pendingStart is not null) throw new InvalidOperationException("The microphone is still starting. Try again shortly or select another device.");
            start = Task.Run(() => OpenSession(deviceIndex), CancellationToken.None);
            _pendingStart = start;
        }

        try
        {
            Session session = await start.WaitAsync(StartTimeout, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                if (!ReferenceEquals(_pendingStart, start)) throw new OperationCanceledException("Recording startup was canceled.");
                _active = session;
                _pendingStart = null;
            }
        }
        catch (Exception ex)
        {
            bool abandon;
            lock (_gate)
            {
                abandon = ReferenceEquals(_pendingStart, start);
                if (abandon && start.IsCompleted) _pendingStart = null;
            }
            if (abandon)
            {
                // A driver can return after timeout. Dispose the late session.
                _ = start.ContinueWith(completed =>
                {
                    try
                    {
                        if (completed.IsCompletedSuccessfully)
                        {
                            try { completed.Result.Capture.StopRecording(); } catch { }
                            CloseSession(completed.Result, deleteAudio: true);
                        }
                    }
                    catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
                    finally
                    {
                        lock (_gate)
                            if (ReferenceEquals(_pendingStart, start)) _pendingStart = null;
                    }
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }
            if (ex is TimeoutException)
                throw new TimeoutException("Microphone startup timed out. Check microphone permissions or select another device.", ex);
            throw;
        }
    }

    public async Task<string> StopAsync(CancellationToken cancellationToken)
    {
        Session session;
        lock (_gate)
        {
            session = _active ?? throw new InvalidOperationException("No recording is in progress.");
            _active = null;
        }
        bool completed = false;
        try
        {
            await Task.Run(session.Capture.StopRecording, cancellationToken).WaitAsync(StopTimeout, cancellationToken);
            await session.Stopped.Task.WaitAsync(StopTimeout, cancellationToken);
            if (!File.Exists(session.Path) || new FileInfo(session.Path).Length <= 44)
                throw new InvalidOperationException("No audio was captured. Check the microphone and try again.");
            completed = true;
            return session.Path;
        }
        finally
        {
            if (completed) CloseSession(session, deleteAudio: false);
            else _ = Task.Run(() => CloseSession(session, deleteAudio: true));
        }
    }

    public Task CancelAsync()
    {
        Session? session;
        lock (_gate)
        {
            session = _active;
            _active = null;
        }
        if (session is null) return Task.CompletedTask;
        return Task.Run(() =>
        {
            try { session.Capture.StopRecording(); } catch { }
            CloseSession(session, deleteAudio: true);
        });
    }

    private static Session OpenSession(int deviceIndex)
    {
        string directory = Path.Combine(Path.GetTempPath(), "VoiceTranslator");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"recording-{Guid.NewGuid():N}.wav");
        var capture = new WaveIn
        {
            DeviceNumber = deviceIndex,
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 50,
            NumberOfBuffers = 3
        };
        Session? session = null;
        try
        {
            session = new Session(capture, new WaveFileWriter(path, capture.WaveFormat), path);
            capture.DataAvailable += session.OnDataAvailable;
            capture.RecordingStopped += session.OnRecordingStopped;
            capture.StartRecording();
            return session;
        }
        catch
        {
            if (session is not null) CloseSession(session, deleteAudio: true);
            else capture.Dispose();
            throw;
        }
    }

    private static void CleanStaleRecordings()
    {
        try
        {
            string directory = Path.Combine(Path.GetTempPath(), "VoiceTranslator");
            if (!Directory.Exists(directory)) return;
            foreach (string path in Directory.EnumerateFiles(directory, "recording-*.wav"))
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(path) < DateTime.UtcNow.AddDays(-1)) File.Delete(path);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static void CloseSession(Session session, bool deleteAudio)
    {
        session.Capture.DataAvailable -= session.OnDataAvailable;
        session.Capture.RecordingStopped -= session.OnRecordingStopped;
        session.Capture.Dispose();
        lock (session.Gate)
        {
            session.Writer?.Dispose();
            session.Writer = null;
        }
        if (deleteAudio && File.Exists(session.Path)) File.Delete(session.Path);
    }

    public void Dispose() => _ = CancelAsync();

    private sealed class Session(WaveIn capture, WaveFileWriter writer, string path)
    {
        public readonly object Gate = new();
        public readonly WaveIn Capture = capture;
        public WaveFileWriter? Writer = writer;
        public readonly string Path = path;
        public readonly TaskCompletionSource Stopped = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            lock (Gate) Writer?.Write(e.Buffer, 0, e.BytesRecorded);
        }

        public void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            lock (Gate)
            {
                Writer?.Dispose();
                Writer = null;
            }
            if (e.Exception is null) Stopped.TrySetResult();
            else Stopped.TrySetException(e.Exception);
        }
    }
}
