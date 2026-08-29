using System;
using System.Collections.Generic;
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

        var resumeOption = new Option<bool>("--resume", "-r")
        {
            Description = "Resume previously copied file selection"
        };
        rootCommand.Options.Add(resumeOption);

        rootCommand.SetAction(parseResult => Run(parseResult.GetValue(resumeOption)));
        return rootCommand.Parse(args).Invoke();
    }

    static void Run(bool resume)
    {
        // show cursor after ctrl+c
        Console.CancelKeyPress += (_, _) => Console.CursorVisible = true;

        string currentDir = ".";
        string root = Path.GetFullPath(currentDir);
        var checkedKeys = resume ? Resume.LoadCheckedKeys(root) : new List<string>();

        string? gitDir = Repository.Discover(Path.GetFullPath("."));
        using var repo = gitDir != null ? new Repository(gitDir) : null;

        bool IsIgnoredPath(string fullPath)
        {
            if (repo == null) return false;
            string relPath = Path.GetRelativePath(repo.Info.WorkingDirectory, fullPath).Replace('\\', '/');
            return repo.Ignore.IsPathIgnored(relPath);
        }

        List<string> CollectAllFiles(string dir)
        {
            var result = new List<string>();
            foreach (var entry in Directory.GetFileSystemEntries(dir))
            {
                string name = Path.GetFileName(entry)!;
                if (name == ".git" || name == ".DS_Store") continue;

                string full = Path.GetFullPath(entry);
                if (IsIgnoredPath(full)) continue;

                if (Directory.Exists(entry))
                {
                    result.AddRange(CollectAllFiles(entry));
                }
                else
                {
                    result.Add(full);
                }
            }
            return result;
        }

        while (true)
        {
            var entries = Directory.GetFileSystemEntries(currentDir)
                .Select(f => Path.GetFileName(f)!)
                .Where(f => f != ".git" && f != ".DS_Store")
                .ToArray();

            if (entries.Length == 0)
            {
                Console.Error.WriteLine("There are no files here.");
                Environment.Exit(1);
            }

            bool IsIgnored(string name) => IsIgnoredPath(Path.GetFullPath(Path.Combine(currentDir, name)));

            var dirs = entries
                .Where(e => Directory.Exists(Path.Combine(currentDir, e)))
                .OrderBy(e => e)
                .Select(e => (Name: e + "/", IsDir: true, IsIgnored: IsIgnored(e), Key: ""));
            var files = entries
                .Where(e => !Directory.Exists(Path.Combine(currentDir, e)))
                .OrderBy(e => e)
                .Select(e => (Name: e, IsDir: false, IsIgnored: IsIgnored(e), Key: Path.GetFullPath(Path.Combine(currentDir, e))));

            var all = dirs.Concat(files);
            var items = all.Where(i => !i.IsIgnored)
                .Concat(all.Where(i => i.IsIgnored))
                .ToList();

            if (currentDir != ".")
            {
                items.Insert(0, (BackCommand, true, false, ""));
            }

            var (selected, copy, selectAll) = Menu.Select(items, checkedKeys);

            if (selectAll)
            {
                foreach (var f in CollectAllFiles(currentDir))
                {
                    if (!checkedKeys.Contains(f)) checkedKeys.Add(f);
                }
                continue;
            }

            if (copy)
            {
                string content = "";
                foreach (var path in checkedKeys)
                {
                    string? result = FileViewer.Show(path);
                    if (result != null) content += result;
                }
                ClipboardService.SetText(content);
                Resume.Save(root, checkedKeys);
                Console.WriteLine($"copied {checkedKeys.Count} file(s) to clipboard");
                break;
            }
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
            }
        }
    }
}
