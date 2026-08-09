using System;
using System.IO;
using System.Text;

class FileViewer
{
    public static string? Show(string path)
    {
        try
        {
            var sb = new StringBuilder();
            string header = $"# === {path} ===";
            Console.WriteLine(header);
            sb.AppendLine(header);

            using var reader = new StreamReader(path);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
                sb.AppendLine(line);
            }
            Console.WriteLine();
            return sb.ToString();
        }
        catch (FileNotFoundException)
        {
            Console.Error.WriteLine($"file not found: {path}");
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"err: {ex.Message}");
            return null;
        }
    }
}
