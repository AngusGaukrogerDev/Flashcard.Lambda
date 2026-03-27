namespace Flashcards.Application.Decks.GetDecks;

public record GetDecksQuery(string UserId, int? PageSize = null, string? PaginationToken = null);
