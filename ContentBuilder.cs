using System.Text;

namespace Licat;

static class ContentBuilder
{
    public static string Build(string root, IEnumerable<string> paths)
    {
        var sb = new StringBuilder();
        foreach (var path in paths)
        {
            try
            {
                // read fully before appending so a failure never leaves a partial file behind
                var lines = File.ReadAllLines(path);
                var rel = Path.GetRelativePath(root, path).Replace('\\', '/');
                sb.AppendLine($"# === {rel} ===");
                foreach (var line in lines) sb.AppendLine(line);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"err: {ex.Message}");
            }
        }
        return sb.ToString();
    }
}
