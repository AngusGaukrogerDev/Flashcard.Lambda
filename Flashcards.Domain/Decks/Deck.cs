namespace Flashcards.Domain.Decks;

public class Deck
{
    private Deck(DeckId id, string name, string? description, DateTime createdAt, string userId)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
        UserId = userId;
    }

    public DeckId Id { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; }
    public string UserId { get; }

    public static Deck Create(string name, string userId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Deck name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new Deck(DeckId.New(), name.Trim(), description?.Trim(), DateTime.UtcNow, userId);
    }

    public static Deck Reconstitute(DeckId id, string name, string? description, DateTime createdAt, string userId)
        => new(id, name, description, createdAt, userId);

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Deck name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
    }
}
