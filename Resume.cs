using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

class ResumeFile
{
    [JsonPropertyName("paths")]
    public Dictionary<string, PathEntry> Paths { get; set; } = new();
}

class PathEntry
{
    [JsonPropertyName("copied")]
    public List<string> Copied { get; set; } = new();
}

static class Resume
{
    static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".licat", "resume.json");

    static ResumeFile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new ResumeFile();
            }
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<ResumeFile>(json) ?? new ResumeFile();
        }
        catch
        {
            return new ResumeFile();
        }
    }

    public static List<string> LoadCheckedKeys(string root)
    {
        var file = Load();
        if (!file.Paths.TryGetValue(root, out var entry))
        {
            return new List<string>();
        }

        return entry.Copied
            .Select(rel => Path.GetFullPath(Path.Combine(root, rel)))
            .Where(File.Exists)
            .ToList();
    }

    public static void Save(string root, List<string> checkedKeys)
    {
        var file = Load();
        file.Paths[root] = new PathEntry
        {
            Copied = checkedKeys
                .Select(key => Path.GetRelativePath(root, key).Replace('\\', '/'))
                .ToList()
        };

        try
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(file, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // コピー自体は既に成功しているので、履歴の保存失敗は無視する
        }
    }
}
