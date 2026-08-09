using System;
using System.IO;

class FileViewer
{
    public static void Show(string path)
    {
        try
        {
            Console.WriteLine($"=== {path} ===");

            using var reader = new StreamReader(path);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();
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
