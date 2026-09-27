using LibGit2Sharp;

namespace Licat;

sealed record Entry(string Name, string FullPath, bool IsDirectory, bool IsIgnored)
{
    public string DisplayName => IsDirectory ? Name + "/" : Name;
}

sealed class FileTree : IDisposable
{
    static readonly HashSet<string> HiddenNames = [".git", ".DS_Store"];

    readonly Repository? _repo;

    FileTree(Repository? repo) => _repo = repo;

    public static FileTree Open(string path)
    {
        string? gitDir = Repository.Discover(path);
        return new FileTree(gitDir != null ? new Repository(gitDir) : null);
    }

    public IReadOnlyList<Entry> List(string dir) =>
        Children(dir)
            .Select(path => new Entry(
                Name: Path.GetFileName(path),
                FullPath: path,
                IsDirectory: Directory.Exists(path),
                IsIgnored: IsIgnored(path)))
            .OrderBy(e => e.IsIgnored)
            .ThenBy(e => !e.IsDirectory)
            .ThenBy(e => e.Name)
            .ToList();

    public IEnumerable<string> EnumerateFiles(string dir)
    {
        foreach (var path in Children(dir))
        {
            if (IsIgnored(path)) continue;

            if (Directory.Exists(path))
            {
                foreach (var file in EnumerateFiles(path)) yield return file;
            }
            else
            {
                yield return path;
            }
        }
    }

    bool IsIgnored(string fullPath)
    {
        if (_repo == null) return false;
        string relPath = Path.GetRelativePath(_repo.Info.WorkingDirectory, fullPath).Replace('\\', '/');
        return _repo.Ignore.IsPathIgnored(relPath);
    }

    static IEnumerable<string> Children(string dir) =>
        Directory.GetFileSystemEntries(dir)
            .Where(path => !HiddenNames.Contains(Path.GetFileName(path)))
            .Select(Path.GetFullPath);

    public void Dispose() => _repo?.Dispose();
}
