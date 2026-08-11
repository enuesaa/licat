using System;
using System.IO;
using System.Linq;
using TextCopy;

class Program
{
    const string BackCommand = "..";

    static void Main()
    {
        string content = "";
        string currentDir = ".";
        while (true)
        {
            var entries = Directory.GetFileSystemEntries(currentDir)
                .Select(f => Path.GetFileName(f)!)
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

            var result = Menu.Select(items, "Please select a file");

            if (result.Action == MenuAction.Exit)
            {
                break;
            }
            if (result.Action == MenuAction.Copy)
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

            string selected = result.Value!;
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
