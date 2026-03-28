namespace Flashcards.Domain.Cards;

using Flashcards.Domain.Users;

public class Card
{
    public const int MaxTextLength = 10000;
    public const int MaxPromptLength = 2000;
    public const int MaxTagsPerCard = 50;

    private readonly List<string> _tagIds;

    private Card(
        CardId id,
        string frontText,
        string backText,
        string deckId,
        UserId userId,
        DateTime createdAt,
        DateTime? nextReviewDate,
        string? frontPrompt,
        string? backPrompt,
        CardColour? backgroundColour,
        TextColour? textColour,
        IEnumerable<string>? tagIds)
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
        _tagIds = tagIds is null ? [] : NormalizeTagIds(tagIds);
    }

    public CardId Id { get; }
    public string FrontText { get; private set; }
    public string BackText { get; private set; }
    public string DeckId { get; }
    public UserId UserId { get; }
    public DateTime CreatedAt { get; }
    public DateTime? NextReviewDate { get; private set; }
    public string? FrontPrompt { get; private set; }
    public string? BackPrompt { get; private set; }
    public CardColour? BackgroundColour { get; private set; }
    public TextColour? TextColour { get; private set; }

    public IReadOnlyList<string> TagIds => _tagIds;

    public static Card Create(
        string frontText,
        string backText,
        string deckId,
        string userId,
        string? frontPrompt = null,
        string? backPrompt = null,
        CardColour? backgroundColour = null,
        TextColour? textColour = null,
        IEnumerable<string>? tagIds = null)
    {
        if (string.IsNullOrWhiteSpace(frontText))
            throw new ArgumentException("Front text cannot be empty.", nameof(frontText));

        if (string.IsNullOrWhiteSpace(backText))
            throw new ArgumentException("Back text cannot be empty.", nameof(backText));

        if (string.IsNullOrWhiteSpace(deckId))
            throw new ArgumentException("Deck ID cannot be empty.", nameof(deckId));

        var trimmedFrontText = frontText.Trim();
        var trimmedBackText = backText.Trim();
        var trimmedFrontPrompt = frontPrompt?.Trim();
        var trimmedBackPrompt = backPrompt?.Trim();

        if (trimmedFrontText.Length > MaxTextLength)
            throw new ArgumentException($"Front text cannot exceed {MaxTextLength} characters.", nameof(frontText));

        if (trimmedBackText.Length > MaxTextLength)
            throw new ArgumentException($"Back text cannot exceed {MaxTextLength} characters.", nameof(backText));

        if (trimmedFrontPrompt is not null && trimmedFrontPrompt.Length > MaxPromptLength)
            throw new ArgumentException($"Front prompt cannot exceed {MaxPromptLength} characters.", nameof(frontPrompt));

        if (trimmedBackPrompt is not null && trimmedBackPrompt.Length > MaxPromptLength)
            throw new ArgumentException($"Back prompt cannot exceed {MaxPromptLength} characters.", nameof(backPrompt));

        return new Card(
            CardId.New(),
            trimmedFrontText,
            trimmedBackText,
            deckId,
            UserId.From(userId),
            DateTime.UtcNow,
            null,
            trimmedFrontPrompt,
            trimmedBackPrompt,
            backgroundColour,
            textColour,
            tagIds);
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
        TextColour? textColour = null,
        IReadOnlyList<string>? tagIds = null)
        => new(id, frontText, backText, deckId, UserId.From(userId), createdAt, nextReviewDate, frontPrompt, backPrompt, backgroundColour, textColour, tagIds);

    public void SetTagIds(IEnumerable<string> tagIds)
    {
        var normalized = NormalizeTagIds(tagIds);
        _tagIds.Clear();
        _tagIds.AddRange(normalized);
    }

    public void RemoveTagId(string tagId)
    {
        if (string.IsNullOrEmpty(tagId))
            return;

        _tagIds.RemoveAll(t => t == tagId);
    }

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

        var trimmedFrontText = frontText.Trim();
        var trimmedBackText = backText.Trim();
        var trimmedFrontPrompt = frontPrompt?.Trim();
        var trimmedBackPrompt = backPrompt?.Trim();

        if (trimmedFrontText.Length > MaxTextLength)
            throw new ArgumentException($"Front text cannot exceed {MaxTextLength} characters.", nameof(frontText));

        if (trimmedBackText.Length > MaxTextLength)
            throw new ArgumentException($"Back text cannot exceed {MaxTextLength} characters.", nameof(backText));

        if (trimmedFrontPrompt is not null && trimmedFrontPrompt.Length > MaxPromptLength)
            throw new ArgumentException($"Front prompt cannot exceed {MaxPromptLength} characters.", nameof(frontPrompt));

        if (trimmedBackPrompt is not null && trimmedBackPrompt.Length > MaxPromptLength)
            throw new ArgumentException($"Back prompt cannot exceed {MaxPromptLength} characters.", nameof(backPrompt));

        FrontText = trimmedFrontText;
        BackText = trimmedBackText;
        FrontPrompt = trimmedFrontPrompt;
        BackPrompt = trimmedBackPrompt;
        BackgroundColour = backgroundColour;
        TextColour = textColour;
    }

    private static List<string> NormalizeTagIds(IEnumerable<string> tagIds)
    {
        var list = new List<string>();
        foreach (var raw in tagIds)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentException("Tag ID cannot be empty.", nameof(tagIds));

            var id = raw.Trim();
            if (!Guid.TryParse(id, out _))
                throw new ArgumentException("Each tag ID must be a valid GUID.", nameof(tagIds));

            if (!list.Contains(id, StringComparer.Ordinal))
                list.Add(id);
        }

        list.Sort(StringComparer.Ordinal);

        if (list.Count > MaxTagsPerCard)
            throw new ArgumentException($"A card cannot have more than {MaxTagsPerCard} tags.", nameof(tagIds));

        return list;
    }
}
