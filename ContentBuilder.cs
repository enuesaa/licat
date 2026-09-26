using System.Text;

namespace Licat;

/// <summary>Concatenates files into a single text, each preceded by a header line.</summary>
static class ContentBuilder
{
    public static string Build(IEnumerable<string> paths)
    {
        var sb = new StringBuilder();
        foreach (var path in paths)
        {
            try
            {
                // read fully before appending so a failure never leaves a partial file behind
                var lines = File.ReadAllLines(path);
                sb.AppendLine($"# === {Path.GetFileName(path)} ===");
                foreach (var line in lines) sb.AppendLine(line);
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
        return sb.ToString();
    }
}
