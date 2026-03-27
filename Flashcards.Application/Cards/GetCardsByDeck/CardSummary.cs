namespace Flashcards.Application.Cards.GetCardsByDeck;

public record CardSummary(
    Guid Id,
    string FrontText,
    string BackText,
    DateTime CreatedAt,
    DateTime? NextReviewDate);
