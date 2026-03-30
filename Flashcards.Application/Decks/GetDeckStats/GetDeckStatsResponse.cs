namespace Flashcards.Application.Decks.GetDeckStats;

public record GetDeckStatsResponse(
    string DeckId,
    int TotalCards,
    int NewCount,
    int IncorrectCount,
    int HardCount,
    int MediumCount,
    int EasyCount);

