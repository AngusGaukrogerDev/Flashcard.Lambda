namespace Flashcards.Application.Cards.GetCardsForStudy;

public record GetCardsForStudyQuery(string DeckId, string UserId, int Limit);
