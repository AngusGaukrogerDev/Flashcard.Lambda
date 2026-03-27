namespace Flashcards.Application.Decks.GetDecks;

public record DeckSummary(Guid Id, string Name, string? Description, DateTime CreatedAt);
