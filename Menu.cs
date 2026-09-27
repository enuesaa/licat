namespace Licat;

enum MenuCommand { Open, Parent, SelectAll, Copy, Exit }

readonly record struct MenuResult(MenuCommand Command, Entry? Entry = null);

static class Menu
{
    const string Help = "c: copy to clipboard, a: select all, ←: parent directory";

    public static MenuResult Show(IReadOnlyList<Entry> entries, Selection selection)
    {
        int index = 0;
        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                int height = Render(entries, index, selection);
                var result = HandleKey(Console.ReadKey(intercept: true).Key, entries, ref index, selection);
                Erase(height);
                if (result is { } r) return r;
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    static MenuResult? HandleKey(ConsoleKey key, IReadOnlyList<Entry> entries, ref int index, Selection selection)
    {
        int count = entries.Count;
        switch (key)
        {
            case ConsoleKey.UpArrow when count > 0:
                index = (index - 1 + count) % count;
                return null;
            case ConsoleKey.DownArrow when count > 0:
                index = (index + 1) % count;
                return null;
            case ConsoleKey.LeftArrow:
                return new(MenuCommand.Parent);
            case ConsoleKey.X:
            case ConsoleKey.Escape:
                return new(MenuCommand.Exit);
            case ConsoleKey.C:
                return new(MenuCommand.Copy);
            case ConsoleKey.A:
                return new(MenuCommand.SelectAll);
            case ConsoleKey.Enter when count > 0:
                var entry = entries[index];
                if (entry.IsDirectory) return new(MenuCommand.Open, entry);
                selection.Toggle(entry.FullPath);
                return null;
            default:
                return null;
        }
    }

    static int Render(IReadOnlyList<Entry> entries, int index, Selection selection)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            Console.Write(i == index ? "> " : "  ");

            if (!entry.IsDirectory)
            {
                Console.Write(selection.Contains(entry.FullPath) ? "[x] " : "[ ] ");
            }

            if (entry.IsIgnored) Console.ForegroundColor = ConsoleColor.DarkGray;
            else if (entry.IsDirectory) Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(entry.DisplayName);
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(Help);
        Console.ResetColor();

        return entries.Count + 2;
    }

    static void Erase(int lines) => Console.Write($"\x1b[{lines}F\x1b[0J");
}
