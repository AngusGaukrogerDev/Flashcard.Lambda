namespace Flashcards.Domain.Decks;

using Flashcards.Domain.Users;

public class Deck
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2000;

    private Deck(DeckId id, string name, string? description, DateTime createdAt, UserId userId)
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
    public UserId UserId { get; }

    public static Deck Create(string name, string userId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Deck name cannot be empty.", nameof(name));

        var trimmedName = name.Trim();
        var trimmedDescription = description?.Trim();

        if (trimmedName.Length > MaxNameLength)
            throw new ArgumentException($"Deck name cannot exceed {MaxNameLength} characters.", nameof(name));

        if (trimmedDescription is not null && trimmedDescription.Length > MaxDescriptionLength)
            throw new ArgumentException($"Deck description cannot exceed {MaxDescriptionLength} characters.", nameof(description));

        return new Deck(DeckId.New(), trimmedName, trimmedDescription, DateTime.UtcNow, UserId.From(userId));
    }

    public static Deck Reconstitute(DeckId id, string name, string? description, DateTime createdAt, string userId)
        => new(id, name, description, createdAt, UserId.From(userId));

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Deck name cannot be empty.", nameof(name));

        var trimmedName = name.Trim();
        var trimmedDescription = description?.Trim();

        if (trimmedName.Length > MaxNameLength)
            throw new ArgumentException($"Deck name cannot exceed {MaxNameLength} characters.", nameof(name));

        if (trimmedDescription is not null && trimmedDescription.Length > MaxDescriptionLength)
            throw new ArgumentException($"Deck description cannot exceed {MaxDescriptionLength} characters.", nameof(description));

        Name = trimmedName;
        Description = trimmedDescription;
    }
}
