using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.GetCardsForStudy;

public record StudyCardItem(
    Guid Id,
    string FrontText,
    string BackText,
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
    IReadOnlyList<string> TagIds,
    bool IsDue);
