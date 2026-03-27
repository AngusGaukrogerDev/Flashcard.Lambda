namespace Flashcards.Domain.Cards;

public class Card
{
    private Card(CardId id, string frontText, string backText, string deckId, string userId, DateTime createdAt, DateTime? nextReviewDate)
    {
        Id = id;
        FrontText = frontText;
        BackText = backText;
        DeckId = deckId;
        UserId = userId;
        CreatedAt = createdAt;
        NextReviewDate = nextReviewDate;
    }

    public CardId Id { get; }
    public string FrontText { get; private set; }
    public string BackText { get; private set; }
    public string DeckId { get; }
    public string UserId { get; }
    public DateTime CreatedAt { get; }
    public DateTime? NextReviewDate { get; private set; }

    public static Card Create(string frontText, string backText, string deckId, string userId)
    {
        if (string.IsNullOrWhiteSpace(frontText))
            throw new ArgumentException("Front text cannot be empty.", nameof(frontText));

        if (string.IsNullOrWhiteSpace(backText))
            throw new ArgumentException("Back text cannot be empty.", nameof(backText));

        if (string.IsNullOrWhiteSpace(deckId))
            throw new ArgumentException("Deck ID cannot be empty.", nameof(deckId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new Card(CardId.New(), frontText.Trim(), backText.Trim(), deckId, userId, DateTime.UtcNow, null);
    }

    public static Card Reconstitute(CardId id, string frontText, string backText, string deckId, string userId, DateTime createdAt, DateTime? nextReviewDate)
        => new(id, frontText, backText, deckId, userId, createdAt, nextReviewDate);

    public void Update(string frontText, string backText)
    {
        if (string.IsNullOrWhiteSpace(frontText))
            throw new ArgumentException("Front text cannot be empty.", nameof(frontText));

        if (string.IsNullOrWhiteSpace(backText))
            throw new ArgumentException("Back text cannot be empty.", nameof(backText));

        FrontText = frontText.Trim();
        BackText = backText.Trim();
    }
}
