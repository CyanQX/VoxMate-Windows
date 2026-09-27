using System.Runtime.InteropServices;

namespace VoiceTranslator.Platform;

public sealed class GlobalHotkeyRegistration : IDisposable
{
    public const int Message = 0x0312;
    public const int Id = 0x5601;
    private const uint ModAlt = 0x0001;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkSpace = 0x20;
    private readonly nint _windowHandle;
    private bool _registered;

    private GlobalHotkeyRegistration(nint windowHandle) => _windowHandle = windowHandle;

    public static GlobalHotkeyRegistration? TryRegister(nint windowHandle)
    {
        var registration = new GlobalHotkeyRegistration(windowHandle);
        if (!RegisterHotKey(windowHandle, Id, ModAlt | ModNoRepeat, VkSpace)) return null;
        registration._registered = true;
        return registration;
    }

    public void Dispose()
    {
        if (!_registered) return;
        UnregisterHotKey(_windowHandle, Id);
        _registered = false;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint windowHandle, int id);
}
