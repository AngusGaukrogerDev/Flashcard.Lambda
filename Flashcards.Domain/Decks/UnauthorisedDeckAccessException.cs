namespace Flashcards.Domain.Decks;

public class UnauthorisedDeckAccessException(string deckId)
    : Exception($"Access to deck '{deckId}' is not permitted.");
