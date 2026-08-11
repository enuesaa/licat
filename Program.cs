using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using TextCopy;

class Program
{
    const string BackCommand = "..";

    static void Main()
    {
        string content = "";
        string currentDir = ".";
        string rootDir = Path.GetFullPath(".");
        using var repo = Repository.IsValid(rootDir) ? new Repository(rootDir) : null;

        while (true)
        {
            var entries = Directory.GetFileSystemEntries(currentDir)
                .Select(f => Path.GetFileName(f)!)
                .Where(f => f != ".git")
                .Where(f =>
                {
                    if (repo == null) return true;
                    string full = Path.GetFullPath(Path.Combine(currentDir, f));
                    string relPath = Path.GetRelativePath(rootDir, full).Replace('\\', '/');
                    return !repo.Ignore.IsPathIgnored(relPath);
                })
                .ToArray();

            if (entries.Length == 0)
            {
                Console.Error.WriteLine("There are no files here.");
                Environment.Exit(1);
            }

            var dirs = entries
                .Where(e => Directory.Exists(Path.Combine(currentDir, e)))
                .OrderBy(e => e)
                .Select(e => (Name: e + "/", IsDir: true));
            var files = entries
                .Where(e => !Directory.Exists(Path.Combine(currentDir, e)))
                .OrderBy(e => e)
                .Select(e => (Name: e, IsDir: false));

            var items = dirs.Concat(files).ToList();
            if (currentDir != ".")
            {
                items.Insert(0, (BackCommand, true));
            }
            items.Add(("@c", false));

            var selected = Menu.Select(items, "Please select");

            if (selected == null)
            {
                break;
            }
            if (selected == "@c")
            {
                if (content == "")
                {
                    Console.Error.WriteLine("There are no contents.");
                    continue;
                }
                ClipboardService.SetText(content);
                Console.WriteLine("Copied to clipboard");
                break;
            }
            if (selected == BackCommand)
            {
                currentDir = Path.GetDirectoryName(currentDir) is { Length: > 0 } parent
                    ? parent
                    : ".";
                continue;
            }

            string fullPath = Path.Combine(currentDir, selected.TrimEnd('/'));
            if (Directory.Exists(fullPath))
            {
                currentDir = fullPath;
                continue;
            }
            content += FileViewer.Show(fullPath);
            currentDir = ".";
        }
    }
}
