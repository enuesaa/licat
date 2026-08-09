using System;
using System.IO;
using System.Linq;
using Sharprompt;
using TextCopy;

class Program
{
    const string CopyCommand = "/copy";

    static void Main()
    {
        // var files = Directory.GetFiles(".")
        //     .Select(f => f.StartsWith("./") ? f[2..] : f)
        //     .ToArray();
        var files = Directory.GetFiles(".", "*", SearchOption.AllDirectories)
            .Select(f => f.StartsWith("./") ? f[2..] : f)
            .ToArray();

        if (files.Length == 0)
        {
            Console.Error.WriteLine("There are no files here.");
            Environment.Exit(1);
        }

        string content = "";

        while (true)
        {
            var items = files.Append(CopyCommand).ToArray();
            string selected = Prompt.Select("Please select a file", items: items);

            // remove prompt
            int line = Console.CursorTop;
            Console.SetCursorPosition(0, line - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, line - 1);

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
            content += FileViewer.Show(selected);
        }
    }
}
