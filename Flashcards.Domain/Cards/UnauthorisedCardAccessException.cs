namespace Flashcards.Domain.Cards;

public class UnauthorisedCardAccessException(string cardId)
    : Exception($"Access to card '{cardId}' is not authorised.");
