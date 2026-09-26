using System.Collections;

namespace Licat;

/// <summary>Checked file paths, kept in the order they were checked.</summary>
sealed class Selection : IEnumerable<string>
{
    readonly List<string> _order = [];
    readonly HashSet<string> _set = [];

    public Selection() { }

    public Selection(IEnumerable<string> paths) => AddRange(paths);

    public int Count => _order.Count;

    public bool Contains(string path) => _set.Contains(path);

    public void Add(string path)
    {
        if (_set.Add(path)) _order.Add(path);
    }

    public void AddRange(IEnumerable<string> paths)
    {
        foreach (var path in paths) Add(path);
    }

    public void Toggle(string path)
    {
        if (_set.Remove(path)) _order.Remove(path);
        else Add(path);
    }

    public IEnumerator<string> GetEnumerator() => _order.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
