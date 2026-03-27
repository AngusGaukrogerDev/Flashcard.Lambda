namespace Flashcards.Application.Cards.GetCardsByDeck;

public record GetCardsByDeckQuery(
    string DeckId,
    string UserId,
    int? PageSize = null,
    string? PaginationToken = null);
