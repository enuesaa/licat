using System;
using System.Collections.Generic;
using System.Linq;

static class Menu
{
    public static string? Select(List<(string Name, bool IsDir, bool IsIgnored)> items, string title)
    {
        string filter = "";
        int index = 0;
        int height = 0;
        bool firstRender = true;
        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                var filtered = items
                    .Where(i => i.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (index >= filtered.Count) index = Math.Max(0, filtered.Count - 1);

                if (!firstRender)
                {
                    Console.Write($"\x1b[{height}F\x1b[0J");
                }
                firstRender = false;

                height = Render(filtered, index, title, filter);

                var key = Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (filtered.Count > 0) index = (index - 1 + filtered.Count) % filtered.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        if (filtered.Count > 0) index = (index + 1) % filtered.Count;
                        break;
                    case ConsoleKey.Backspace:
                        if (filter.Length > 0)
                        {
                            filter = filter[..^1];
                            index = 0;
                        }
                        break;
                    case ConsoleKey.Escape:
                        if (filter.Length > 0)
                        {
                            filter = "";
                            index = 0;
                        }
                        else
                        {
                            Console.Write($"\x1b[{height}F\x1b[0J");
                            return null;
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (filtered.Count > 0)
                        {
                            Console.Write($"\x1b[{height}F\x1b[0J");
                            return filtered[index].Name;
                        }
                        break;
                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            if (key.KeyChar == '/')
                            {
                                var match = items.FirstOrDefault(i =>
                                    i.IsDir && string.Equals(i.Name.TrimEnd('/'), filter, StringComparison.OrdinalIgnoreCase));

                                if (match.Name != null)
                                {
                                    Console.Write($"\x1b[{height}F\x1b[0J");
                                    return match.Name;
                                }
                            }

                            filter += key.KeyChar;
                            index = 0;
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

    static int Render(List<(string Name, bool IsDir, bool IsIgnored)> filtered, int index, string title, string filter)
    {
        int lines = 0;
        Console.WriteLine($"{title}: {filter}");
        lines++;

        if (filtered.Count == 0)
        {
            Console.WriteLine("  no matches");
            lines++;
        }
        else
        {
            for (int i = 0; i < filtered.Count; i++)
            {
                var (name, isDir, isIgnored) = filtered[i];
                Console.Write(i == index ? "> " : "  ");
                if (isIgnored) Console.ForegroundColor = ConsoleColor.DarkGray;
                else if (isDir) Console.ForegroundColor = ConsoleColor.Cyan;
                else if (name == "@c") Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(name);
                Console.ResetColor();
                Console.WriteLine();
                lines++;
            }
        }

        return lines;
    }
}
