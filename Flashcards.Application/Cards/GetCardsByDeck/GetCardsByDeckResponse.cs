namespace Flashcards.Application.Cards.GetCardsByDeck;

public record GetCardsByDeckResponse(string DeckId, IReadOnlyList<CardSummary> Cards, string? NextPaginationToken);
