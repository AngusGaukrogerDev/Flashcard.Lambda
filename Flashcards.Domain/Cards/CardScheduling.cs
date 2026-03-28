namespace Flashcards.Domain.Cards;

/// <summary>
/// SM-2–inspired scheduling for four recall levels. Incorrect triggers a short relearn delay.
/// </summary>
public static class CardScheduling
{
    public const double DefaultEaseFactor = 2.5;
    public const double MinEaseFactor = 1.3;
    public const double MaxEaseFactor = 3.0;
    public const int RelearnMinutes = 10;

    public static (double EaseFactor, double IntervalDays, int RepetitionCount, DateTime NextReviewUtc) CalculateNext(
        RecallRating rating,
        DateTime nowUtc,
        double easeFactor,
        double intervalDays,
        int repetitionCount)
    {
        if (rating == RecallRating.Incorrect)
        {
            var lapseEase = Math.Max(MinEaseFactor, easeFactor - 0.2);
            var next = nowUtc.AddMinutes(RelearnMinutes);
            return (lapseEase, 0, 0, next);
        }

        double newEase;
        double newIntervalDays;
        int newRepetitionCount;

        if (repetitionCount == 0)
        {
            newIntervalDays = rating switch
            {
                RecallRating.Hard => 0.5,
                RecallRating.Medium => 1.0,
                RecallRating.Easy => 2.0,
                _ => 1.0
            };
            newRepetitionCount = 1;
        }
        else
        {
            var mult = rating switch
            {
                RecallRating.Hard => 0.5,
                RecallRating.Medium => 1.0,
                RecallRating.Easy => 1.3,
                _ => 1.0
            };
            var previous = Math.Max(intervalDays, 1e-6);
            newIntervalDays = previous * easeFactor * mult;
            newRepetitionCount = repetitionCount + 1;
        }

        newEase = rating switch
        {
            RecallRating.Hard => Math.Max(MinEaseFactor, easeFactor - 0.15),
            RecallRating.Medium => easeFactor,
            RecallRating.Easy => Math.Min(MaxEaseFactor, easeFactor + 0.15),
            _ => easeFactor
        };

        var nextReview = nowUtc.AddDays(newIntervalDays);
        return (newEase, newIntervalDays, newRepetitionCount, nextReview);
    }
}
