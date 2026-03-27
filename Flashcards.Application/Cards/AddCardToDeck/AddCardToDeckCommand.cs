namespace Flashcards.Application.Cards.AddCardToDeck;

public record AddCardToDeckCommand(string FrontText, string BackText, string DeckId, string UserId);
