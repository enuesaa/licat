using System;
using System.Collections.Generic;

enum MenuAction { Select, Copy, Exit }

readonly struct MenuResult
{
    public MenuAction Action { get; init; }
    public string? Value { get; init; }
}

static class Menu
{
    public static MenuResult Select(List<(string Name, bool IsDir)> items, string title)
    {
        int index = 0;
        bool firstRender = true;
        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                if (!firstRender)
                {
                    // 直前の描画行数ぶんだけ相対的に巻き戻す(絶対座標は使わない)
                    Console.SetCursorPosition(0, Console.CursorTop - (items.Count + 2));
                }
                firstRender = false;

                Render(items, index, title);

                var key = Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        index = (index - 1 + items.Count) % items.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        index = (index + 1) % items.Count;
                        break;
                    case ConsoleKey.Enter:
                        ClearCurrent(items.Count);
                        return new MenuResult { Action = MenuAction.Select, Value = items[index].Name };
                    case ConsoleKey.C:
                        ClearCurrent(items.Count);
                        return new MenuResult { Action = MenuAction.Copy };
                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        ClearCurrent(items.Count);
                        return new MenuResult { Action = MenuAction.Exit };
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    static void Render(List<(string Name, bool IsDir)> items, int index, string title)
    {
        Console.WriteLine(Pad(title));
        for (int i = 0; i < items.Count; i++)
        {
            var (name, isDir) = items[i];
            Console.Write(i == index ? "> " : "  ");
            if (isDir) Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(name);
            Console.ResetColor();
            Console.WriteLine(new string(' ', Math.Max(0, Console.WindowWidth - name.Length - 2)));
        }
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(Pad("[enter] open   [c] copy   [q] exit"));
        Console.ResetColor();
    }

    static string Pad(string s) => s.PadRight(Math.Max(s.Length, Console.WindowWidth));

    static void ClearCurrent(int itemCount)
    {
        // 描画した分だけ上に戻ってから空白で消す
        Console.SetCursorPosition(0, Console.CursorTop - (itemCount + 2));
        for (int i = 0; i < itemCount + 2; i++)
        {
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine();
        }
        Console.SetCursorPosition(0, Console.CursorTop - (itemCount + 2));
    }
}
