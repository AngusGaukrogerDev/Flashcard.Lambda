namespace Flashcards.Domain.Decks;

public class Deck
{
    private Deck(DeckId id, string name, string? description, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
    }

    public DeckId Id { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; }

    public static Deck Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Deck name cannot be empty.", nameof(name));

        return new Deck(DeckId.New(), name.Trim(), description?.Trim(), DateTime.UtcNow);
    }

    public static Deck Reconstitute(DeckId id, string name, string? description, DateTime createdAt)
        => new(id, name, description, createdAt);
}
