namespace Flashcards.Application.Decks.DeleteDeck;

public record DeleteDeckCommand(string DeckId, string UserId);
