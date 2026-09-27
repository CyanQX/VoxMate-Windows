using System.Runtime.InteropServices;
using System.Text;

namespace VoiceTranslator.Whisper;

internal static class ChineseTextNormalizer
{
    private const uint SimplifiedChinese = 0x02000000;

    public static string ToSimplified(string text)
    {
        if (string.IsNullOrEmpty(text) || !OperatingSystem.IsWindows()) return text;
        int length = LCMapStringEx("zh-CN", SimplifiedChinese, text, text.Length,
            null, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (length <= 0) return text;
        var output = new StringBuilder(length);
        int written = LCMapStringEx("zh-CN", SimplifiedChinese, text, text.Length,
            output, output.Capacity, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        return written > 0 ? output.ToString(0, written).Replace('麽', '么') : text;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int LCMapStringEx(
        string localeName,
        uint mapFlags,
        string source,
        int sourceLength,
        StringBuilder? destination,
        int destinationLength,
        IntPtr versionInformation,
        IntPtr reserved,
        IntPtr sortHandle);
}
