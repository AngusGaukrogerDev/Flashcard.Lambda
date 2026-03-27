namespace Flashcards.Application.Decks.UpdateDeck;

public record UpdateDeckResponse(Guid Id, string Name, string? Description, DateTime CreatedAt);
