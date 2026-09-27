using TextCopy;

namespace Licat;

sealed class App(string root, FileTree tree, Selection selection, ResumeStore store)
{
    readonly string _root = root;
    string _currentDir = root;

    public int Run()
    {
        while (true)
        {
            var entries = tree.List(_currentDir);
            if (entries.Count == 0)
            {
                Console.Error.WriteLine("There are no files here.");
                return 1;
            }

            var result = Menu.Show(entries, selection);
            switch (result.Command)
            {
                case MenuCommand.Open:
                    _currentDir = result.Entry!.FullPath;
                    break;
                case MenuCommand.Parent:
                    GoToParent();
                    break;
                case MenuCommand.SelectAll:
                    selection.AddRange(tree.EnumerateFiles(_currentDir));
                    break;
                case MenuCommand.Copy:
                    Copy();
                    return 0;
                case MenuCommand.Exit:
                    return 0;
            }
        }
    }

    void GoToParent()
    {
        if (_currentDir == _root) return;
        _currentDir = Path.GetDirectoryName(_currentDir)!;
    }

    void Copy()
    {
        ClipboardService.SetText(ContentBuilder.Build(selection));
        store.Save(_root, selection);
        Console.WriteLine($"copied {selection.Count} file(s) to clipboard");
    }
}
