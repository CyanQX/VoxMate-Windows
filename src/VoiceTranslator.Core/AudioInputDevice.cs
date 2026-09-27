namespace VoiceTranslator.Core;

public sealed record AudioInputDevice(int Index, string Name)
{
    public override string ToString() => Name;
}
