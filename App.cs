using TextCopy;

namespace Licat;

/// <summary>Interactive browse loop: navigate directories, select files, copy them.</summary>
sealed class App(string root, FileTree tree, Selection selection, ResumeStore store)
{
    string? _currentDir;

    string CurrentDir => _currentDir ?? root;

    public int Run()
    {
        while (true)
        {
            var entries = tree.List(CurrentDir);
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
                    selection.AddRange(tree.EnumerateFiles(CurrentDir));
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
        // never navigate above the directory licat was started in
        if (CurrentDir == root) return;
        _currentDir = Path.GetDirectoryName(CurrentDir);
    }

    void Copy()
    {
        ClipboardService.SetText(ContentBuilder.Build(selection));
        store.Save(root, selection);
        Console.WriteLine($"copied {selection.Count} file(s) to clipboard");
    }
}
