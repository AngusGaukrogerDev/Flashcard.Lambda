namespace Flashcards.Application.Decks.GetDecks;

public record GetDecksResponse(IReadOnlyList<DeckSummary> Decks, string? NextPaginationToken);
