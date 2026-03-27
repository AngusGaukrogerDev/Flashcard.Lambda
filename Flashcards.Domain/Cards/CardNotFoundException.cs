namespace Flashcards.Domain.Cards;

public class CardNotFoundException(string cardId)
    : Exception($"Card with ID '{cardId}' was not found.");
