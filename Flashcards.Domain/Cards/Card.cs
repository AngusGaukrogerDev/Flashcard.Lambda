namespace Flashcards.Domain.Cards;

public class Card
{
    private Card(
        CardId id,
        string frontText,
        string backText,
        string deckId,
        string userId,
        DateTime createdAt,
        DateTime? nextReviewDate,
        string? frontPrompt,
        string? backPrompt,
        CardColour? backgroundColour,
        TextColour? textColour)
    {
        Id = id;
        FrontText = frontText;
        BackText = backText;
        DeckId = deckId;
        UserId = userId;
        CreatedAt = createdAt;
        NextReviewDate = nextReviewDate;
        FrontPrompt = frontPrompt;
        BackPrompt = backPrompt;
        BackgroundColour = backgroundColour;
        TextColour = textColour;
    }

    public CardId Id { get; }
    public string FrontText { get; private set; }
    public string BackText { get; private set; }
    public string DeckId { get; }
    public string UserId { get; }
    public DateTime CreatedAt { get; }
    public DateTime? NextReviewDate { get; private set; }
    public string? FrontPrompt { get; private set; }
    public string? BackPrompt { get; private set; }
    public CardColour? BackgroundColour { get; private set; }
    public TextColour? TextColour { get; private set; }

    public static Card Create(
        string frontText,
        string backText,
        string deckId,
        string userId,
        string? frontPrompt = null,
        string? backPrompt = null,
        CardColour? backgroundColour = null,
        TextColour? textColour = null)
    {
        if (string.IsNullOrWhiteSpace(frontText))
            throw new ArgumentException("Front text cannot be empty.", nameof(frontText));

        if (string.IsNullOrWhiteSpace(backText))
            throw new ArgumentException("Back text cannot be empty.", nameof(backText));

        if (string.IsNullOrWhiteSpace(deckId))
            throw new ArgumentException("Deck ID cannot be empty.", nameof(deckId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new Card(
            CardId.New(),
            frontText.Trim(),
            backText.Trim(),
            deckId,
            userId,
            DateTime.UtcNow,
            null,
            frontPrompt?.Trim(),
            backPrompt?.Trim(),
            backgroundColour,
            textColour);
    }

    public static Card Reconstitute(
        CardId id,
        string frontText,
        string backText,
        string deckId,
        string userId,
        DateTime createdAt,
        DateTime? nextReviewDate,
        string? frontPrompt = null,
        string? backPrompt = null,
        CardColour? backgroundColour = null,
        TextColour? textColour = null)
        => new(id, frontText, backText, deckId, userId, createdAt, nextReviewDate, frontPrompt, backPrompt, backgroundColour, textColour);

    public void Update(
        string frontText,
        string backText,
        string? frontPrompt = null,
        string? backPrompt = null,
        CardColour? backgroundColour = null,
        TextColour? textColour = null)
    {
        if (string.IsNullOrWhiteSpace(frontText))
            throw new ArgumentException("Front text cannot be empty.", nameof(frontText));

        if (string.IsNullOrWhiteSpace(backText))
            throw new ArgumentException("Back text cannot be empty.", nameof(backText));

        FrontText = frontText.Trim();
        BackText = backText.Trim();
        FrontPrompt = frontPrompt?.Trim();
        BackPrompt = backPrompt?.Trim();
        BackgroundColour = backgroundColour;
        TextColour = textColour;
    }
}
