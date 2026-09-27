using System.Text.Json;
using VoiceTranslator.Core;

namespace VoiceTranslator.Infrastructure;

public sealed class JsonHistoryService : IHistoryService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VoiceTranslator", "history.json");

    public IReadOnlyList<HistoryEntry> Load()
    {
        if (!File.Exists(_path)) return [];
        try { return JsonSerializer.Deserialize<List<HistoryEntry>>(File.ReadAllText(_path), Options) ?? []; }
        catch (JsonException) { return []; }
    }

    public void Add(HistoryEntry entry)
    {
        var entries = Load().ToList();
        entries.Insert(0, entry);
        Save(entries);
    }

    public void Delete(Guid id) => Save(Load().Where(entry => entry.Id != id).ToList());
    public void Clear() => Save([]);

    private void Save(List<HistoryEntry> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        string temporaryPath = _path + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(entries, Options));
        File.Move(temporaryPath, _path, true);
    }
}
