namespace Flashcards.Application.Cards.GetCardById;

public record GetCardByIdResponse(
    Guid Id,
    string FrontText,
    string BackText,
    string DeckId,
    DateTime CreatedAt,
    DateTime? NextReviewDate);
