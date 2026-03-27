namespace Flashcards.Application.Cards.AddCardToDeck;

public record AddCardToDeckResponse(Guid Id, string FrontText, string BackText, string DeckId, DateTime CreatedAt);
