namespace Flashcards.Application.DeckTags.GetDeckTags;

public record DeckTagSummary(Guid Id, string DeckId, string Name, DateTime CreatedAt);
