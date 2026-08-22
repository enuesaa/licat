using System;
using System.Collections.Generic;
using System.Linq;

static class Menu
{
    public static string? Select(
        List<(string Name, bool IsDir, bool IsIgnored, string Key)> items,
        List<string> checkedKeys)
    {
        int index = 0;
        int height = 0;
        bool firstRender = true;
        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                if (index >= items.Count) index = Math.Max(0, items.Count - 1);

                if (!firstRender)
                {
                    Console.Write($"\x1b[{height}F\x1b[0J");
                }
                firstRender = false;

                height = Render(items, index, checkedKeys);

                var key = Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (items.Count > 0) index = (index - 1 + items.Count) % items.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        if (items.Count > 0) index = (index + 1) % items.Count;
                        break;
                    case ConsoleKey.Escape:
                        Console.Write($"\x1b[{height}F\x1b[0J");
                        return null;
                    case ConsoleKey.Q:
                        Console.Write($"\x1b[{height}F\x1b[0J");
                        return null;
                    case ConsoleKey.Spacebar:
                        ToggleCheck(items, index, checkedKeys);
                        break;
                    case ConsoleKey.Enter:
                        if (items.Count > 0)
                        {
                            var item = items[index];
                            if (item.IsDir)
                            {
                                Console.Write($"\x1b[{height}F\x1b[0J");
                                return item.Name;
                            }
                            ToggleCheck(items, index, checkedKeys);
                        }
                        break;
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    static void ToggleCheck(List<(string Name, bool IsDir, bool IsIgnored, string Key)> items, int index, List<string> checkedKeys)
    {
        if (items.Count == 0) return;
        var item = items[index];
        if (item.IsDir || item.Key == "") return;

        if (!checkedKeys.Remove(item.Key))
        {
            checkedKeys.Add(item.Key);
        }

        string content = "";
        foreach (var path in checkedKeys)
        {
            string? result = FileViewer.Show(path);
            if (result != null) content += result;
        }
        TextCopy.ClipboardService.SetText(content);
    }

    static int Render(List<(string Name, bool IsDir, bool IsIgnored, string Key)> items, int index, List<string> checkedKeys)
    {
        int lines = 0;

        for (int i = 0; i < items.Count; i++)
        {
            var (name, isDir, isIgnored, key) = items[i];
            Console.Write(i == index ? "> " : "  ");

            if (!isDir)
            {
                Console.Write(checkedKeys.Contains(key) ? "[x] " : "[ ] ");
            }

            if (isIgnored) Console.ForegroundColor = ConsoleColor.DarkGray;
            else if (isDir) Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(name);
            Console.ResetColor();
            Console.WriteLine();
            lines++;
        }

        if (checkedKeys.Count > 0)
        {
            Console.WriteLine($"{checkedKeys.Count} file(s) copied to clipboard");
            lines++;
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("q: quit");
        Console.ResetColor();
        lines++;

        return lines;
    }
}
