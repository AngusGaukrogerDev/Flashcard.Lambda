using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.UpdateCard;

public record UpdateCardResponse(
    Guid Id,
    string FrontText,
    string BackText,
    string DeckId,
    DateTime CreatedAt,
    DateTime? NextReviewDate,
    string? FrontPrompt,
    string? BackPrompt,
    CardColour? BackgroundColour,
    TextColour? TextColour);
