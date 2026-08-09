using System;
using System.IO;
using System.Text;
using TextCopy;

class FileViewer
{
    public static void Show(string path)
    {
        try
        {
            var sb = new StringBuilder();
            Console.WriteLine($"=== {path} ===");

            using var reader = new StreamReader(path);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
                sb.AppendLine(line);
            }
            Console.WriteLine();

            ClipboardService.SetText(sb.ToString());
            Console.WriteLine("(clipboard へコピーしました)");
        }
        catch (FileNotFoundException)
        {
            Console.Error.WriteLine($"file not found: {path}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"err: {ex.Message}");
        }
    }
}
