using System;
using System.IO;
using System.Linq;
using Sharprompt;
using TextCopy;

class Program
{
    const string CopyCommand = "/copy";
    const string ExitCommand = "/exit";
    const string BackCommand = "..";

    static void Main()
    {
        string content = "";
        string currentDir = ".";

        while (true)
        {
            var entries = Directory.GetFileSystemEntries(currentDir)
                .Select(f => Path.GetFileName(f))
                .OrderBy(f => f)
                .ToArray();

            if (entries.Length == 0)
            {
                Console.Error.WriteLine("There are no files here.");
                Environment.Exit(1);
            }

            var items = entries
                .Select(e => Directory.Exists(Path.Combine(currentDir, e)) ? e + "/" : e)
                .ToList();
            if (currentDir != ".")
            {
                items.Insert(0, BackCommand);
            }
            items.Add(CopyCommand);
            items.Add(ExitCommand);

            string selected = Prompt.Select("Please select a file", items: items);

            // remove prompt
            int line = Console.CursorTop;
            Console.SetCursorPosition(0, line - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, line - 1);

            if (selected == ExitCommand)
            {
                break;
            }

            if (selected == CopyCommand)
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
        }
    }
}
