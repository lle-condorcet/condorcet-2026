using XssLabApp.Models;

namespace XssLabApp.Services;

/// <summary>
/// Stockage en memoire des captures d'ecran exfiltrees.
/// </summary>
public class CaptureStore
{
    private readonly List<CapturedData> _captures = [];
    private int _nextId = 1;

    public List<CapturedData> GetAll() => [.. _captures];

    public void Add(CapturedData capture)
    {
        capture.Id = _nextId++;
        _captures.Add(capture);
    }

    public void Clear()
    {
        _captures.Clear();
        _nextId = 1;
    }
}
