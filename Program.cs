using System;
using System.CommandLine;
using System.CommandLine.Help;
using System.Globalization;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using TextCopy;

class Program
{
    const string BackCommand = "..";

    static int Main(string[] args)
    {
        // do not translate
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        var rootCommand = new RootCommand("licat --- browse files and copy their contents");

        var helpOption = rootCommand.Options.OfType<HelpOption>().First();
        helpOption.Aliases.Remove("-?");
        helpOption.Aliases.Remove("/?");

        rootCommand.SetAction(_ => Run());
        return rootCommand.Parse(args).Invoke();
    }

    static void Run()
    {
        // show cursor after ctrl+c
        Console.CancelKeyPress += (_, _) => Console.CursorVisible = true;

        string content = "";
        string currentDir = ".";

        string? gitDir = Repository.Discover(Path.GetFullPath("."));
        using var repo = gitDir != null ? new Repository(gitDir) : null;

        while (true)
        {
            var entries = Directory.GetFileSystemEntries(currentDir)
                .Select(f => Path.GetFileName(f)!)
                .Where(f => f != ".git")
                .ToArray();

            if (entries.Length == 0)
            {
                Console.Error.WriteLine("There are no files here.");
                Environment.Exit(1);
            }

            bool IsIgnored(string name)
            {
                if (repo == null) return false;
                string full = Path.GetFullPath(Path.Combine(currentDir, name));
                string relPath = Path.GetRelativePath(repo.Info.WorkingDirectory, full).Replace('\\', '/');
                return repo.Ignore.IsPathIgnored(relPath);
            }

            var dirs = entries
                .Where(e => Directory.Exists(Path.Combine(currentDir, e)))
                .OrderBy(e => e)
                .Select(e => (Name: e + "/", IsDir: true, IsIgnored: IsIgnored(e)));
            var files = entries
                .Where(e => !Directory.Exists(Path.Combine(currentDir, e)))
                .OrderBy(e => e)
                .Select(e => (Name: e, IsDir: false, IsIgnored: IsIgnored(e)));

            var all = dirs.Concat(files);
            var items = all.Where(i => !i.IsIgnored)
                .Concat(all.Where(i => i.IsIgnored))
                .ToList();

            if (currentDir != ".")
            {
                items.Insert(0, (BackCommand, true, false));
            }

            var selected = Menu.Select(items, "Select");

            if (selected == null)
            {
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

            string? result = FileViewer.Show(fullPath);
            if (result != null)
            {
                content += result;
                ClipboardService.SetText(content);
                Console.WriteLine($"copy to clipboard: {selected}");
            }
            currentDir = ".";
        }
    }
}
