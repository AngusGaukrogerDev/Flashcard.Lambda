namespace Flashcards.Application.Decks.UpdateDeck;

public record UpdateDeckCommand(string DeckId, string UserId, string Name, string? Description);
