namespace Flashcards.Domain.Decks;

public class DeckNotFoundException(string deckId)
    : Exception($"Deck with ID '{deckId}' was not found.");
