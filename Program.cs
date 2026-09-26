using System.CommandLine;
using System.CommandLine.Help;
using System.Globalization;

namespace Licat;

static class Program
{
    static int Main(string[] args)
    {
        // do not translate
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        var rootCommand = new RootCommand("licat --- browse files and copy their contents");

        var helpOption = rootCommand.Options.OfType<HelpOption>().First();
        helpOption.Aliases.Remove("-?");
        helpOption.Aliases.Remove("/?");

        var resumeOption = new Option<bool>("--resume", "-r")
        {
            Description = "Resume previously copied file selection"
        };
        rootCommand.Options.Add(resumeOption);

        rootCommand.SetAction(parseResult => Run(parseResult.GetValue(resumeOption)));
        return rootCommand.Parse(args).Invoke();
    }

    static int Run(bool resume)
    {
        // show cursor after ctrl+c
        Console.CancelKeyPress += (_, _) => Console.CursorVisible = true;

        string root = Path.GetFullPath(".");
        var store = ResumeStore.Default;
        var selection = resume ? new Selection(store.Load(root)) : new Selection();

        using var tree = FileTree.Open(root);
        return new App(root, tree, selection, store).Run();
    }
}
