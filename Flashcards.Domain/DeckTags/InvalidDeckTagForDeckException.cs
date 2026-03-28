namespace Flashcards.Domain.DeckTags;

public class InvalidDeckTagForDeckException(string tagId, string deckId)
    : ArgumentException($"Tag '{tagId}' is not a valid tag for deck '{deckId}'.");
