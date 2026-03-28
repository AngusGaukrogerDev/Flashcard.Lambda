using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.GetCardById;

public record GetCardByIdResponse(
    Guid Id,
    string FrontText,
    string BackText,
    string DeckId,
    DateTime CreatedAt,
    DateTime? NextReviewDate,
    double EaseFactor,
    double IntervalDays,
    int RepetitionCount,
    DateTime? LastReviewedAt,
    RecallRating? LastRecallRating,
    RecallTrafficLight? RecallTrafficLight,
    string? FrontPrompt,
    string? BackPrompt,
    CardColour? BackgroundColour,
    TextColour? TextColour,
    IReadOnlyList<string> TagIds);
