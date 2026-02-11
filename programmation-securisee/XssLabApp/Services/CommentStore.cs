using XssLabApp.Models;

namespace XssLabApp.Services;

/// <summary>
/// Stockage en memoire des commentaires du livre d'or (simule une BDD).
/// </summary>
public class CommentStore
{
    private readonly List<Comment> _comments = [];
    private int _nextId = 1;

    public CommentStore()
    {
        // Commentaires legitimes par defaut
        _comments.Add(new Comment
        {
            Id = _nextId++,
            Author = "Alice",
            Content = "Super site, bravo !",
            PostedAt = DateTime.Now.AddHours(-2)
        });
        _comments.Add(new Comment
        {
            Id = _nextId++,
            Author = "Bob",
            Content = "Le cours de cybersecurite est passionnant.",
            PostedAt = DateTime.Now.AddHours(-1)
        });
    }

    public List<Comment> GetAll() => [.. _comments];

    public void Add(Comment comment)
    {
        comment.Id = _nextId++;
        _comments.Add(comment);
    }

    public void Clear()
    {
        _comments.Clear();
        _nextId = 1;
    }
}
