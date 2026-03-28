namespace Flashcards.Domain.DeckTags;

using Flashcards.Domain.Users;

public class DeckTag
{
    public const int MaxNameLength = 100;
    public const int MaxTagsPerDeck = 500;

    private DeckTag(DeckTagId id, string deckId, string name, UserId userId, DateTime createdAt)
    {
        Id = id;
        DeckId = deckId;
        Name = name;
        UserId = userId;
        CreatedAt = createdAt;
    }

    public DeckTagId Id { get; }
    public string DeckId { get; }
    public string Name { get; private set; }
    public UserId UserId { get; }
    public DateTime CreatedAt { get; }

    public static DeckTag Create(string deckId, string userId, string name)
    {
        if (string.IsNullOrWhiteSpace(deckId))
            throw new ArgumentException("Deck ID cannot be empty.", nameof(deckId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty.", nameof(name));

        var trimmed = name.Trim();

        if (trimmed.Length > MaxNameLength)
            throw new ArgumentException($"Tag name cannot exceed {MaxNameLength} characters.", nameof(name));

        return new DeckTag(DeckTagId.New(), deckId, trimmed, UserId.From(userId), DateTime.UtcNow);
    }

    public static DeckTag Reconstitute(DeckTagId id, string deckId, string name, string userId, DateTime createdAt)
        => new(id, deckId, name, UserId.From(userId), createdAt);

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty.", nameof(name));

        var trimmed = name.Trim();

        if (trimmed.Length > MaxNameLength)
            throw new ArgumentException($"Tag name cannot exceed {MaxNameLength} characters.", nameof(name));

        Name = trimmed;
    }
}
