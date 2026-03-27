namespace Flashcards.Application.Cards.UpdateCard;

public record UpdateCardResponse(
    Guid Id,
    string FrontText,
    string BackText,
    string DeckId,
    DateTime CreatedAt,
    DateTime? NextReviewDate);
