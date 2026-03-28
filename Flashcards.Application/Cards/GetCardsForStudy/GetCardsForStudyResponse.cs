namespace Flashcards.Application.Cards.GetCardsForStudy;

public record GetCardsForStudyResponse(
    string DeckId,
    IReadOnlyList<StudyCardItem> Cards,
    int DueCount,
    int UpcomingCount);
