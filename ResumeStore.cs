using System.Text.Json;
using System.Text.Json.Serialization;

namespace Licat;

/// <summary>
/// Persists the last copied selection per root directory in <c>~/.licat/resume.json</c>.
/// Paths are stored relative to the root.
/// </summary>
sealed class ResumeStore(string filePath)
{
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static ResumeStore Default { get; } = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".licat", "resume.json"));

    /// <summary>Absolute paths of previously copied files under <paramref name="root"/> that still exist.</summary>
    public IEnumerable<string> Load(string root)
    {
        if (!Read().Paths.TryGetValue(root, out var entry)) return [];

        return entry.Copied
            .Select(rel => Path.GetFullPath(Path.Combine(root, rel)))
            .Where(File.Exists)
            .ToList();
    }

    public void Save(string root, IEnumerable<string> paths)
    {
        var file = Read();
        file.Paths[root] = new RootEntry
        {
            Copied = paths.Select(path => Path.GetRelativePath(root, path).Replace('\\', '/')).ToList()
        };

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, JsonSerializer.Serialize(file, JsonOptions));
        }
        catch
        {
            // コピー自体は既に成功しているので、履歴の保存失敗は無視する
        }
    }

    ResumeFile Read()
    {
        try
        {
            if (!File.Exists(filePath)) return new ResumeFile();
            return JsonSerializer.Deserialize<ResumeFile>(File.ReadAllText(filePath)) ?? new ResumeFile();
        }
        catch
        {
            return new ResumeFile();
        }
    }

    sealed class ResumeFile
    {
        [JsonPropertyName("paths")]
        public Dictionary<string, RootEntry> Paths { get; set; } = [];
    }

    sealed class RootEntry
    {
        [JsonPropertyName("copied")]
        public List<string> Copied { get; set; } = [];
    }
}
