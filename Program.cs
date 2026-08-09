using System;
using System.IO;
using Sharprompt;

class Program
{
    static void Main()
    {
        var files = Directory.GetFiles(".");

        if (files.Length == 0)
        {
            Console.Error.WriteLine("There are no files here.");
            Environment.Exit(1);
        }

        while (true)
        {
            string selected = Prompt.Select("Please select a file", items: files);
            // remove prompt
            int line = Console.CursorTop;
            Console.SetCursorPosition(0, line - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, line - 1);
            // stdout file
            FileViewer.Show(selected);
        }
    }
}
