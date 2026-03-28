using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.GetCardsByDeck;

public record CardSummary(
    Guid Id,
    string FrontText,
    string BackText,
    DateTime CreatedAt,
    DateTime? NextReviewDate,
    string? FrontPrompt,
    string? BackPrompt,
    CardColour? BackgroundColour,
    TextColour? TextColour,
    IReadOnlyList<string> TagIds);
