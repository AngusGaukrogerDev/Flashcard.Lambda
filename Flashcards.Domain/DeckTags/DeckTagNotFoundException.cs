namespace Flashcards.Domain.DeckTags;

public class DeckTagNotFoundException(string tagId)
    : Exception($"Deck tag with ID '{tagId}' was not found.");
