using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards;

public static class StudyQueueOrdering
{
    public const int MaxLimit = 50;

    /// <summary>
    /// Due cards first (null <see cref="Card.NextReviewDate"/> first, then by date ascending), then upcoming by next review ascending.
    /// </summary>
    public static (IReadOnlyList<Card> Cards, int DueCount, int UpcomingCount) SelectCards(
        IReadOnlyList<Card> all,
        DateTime nowUtc,
        int limit)
    {
        if (limit < 0)
            limit = 0;
        if (limit > MaxLimit)
            limit = MaxLimit;

        var due = all.Where(c => c.NextReviewDate is null || c.NextReviewDate <= nowUtc).ToList();
        var upcoming = all.Where(c => c.NextReviewDate.HasValue && c.NextReviewDate > nowUtc).ToList();

        due.Sort(CompareDue);
        upcoming.Sort((a, b) => a.NextReviewDate!.Value.CompareTo(b.NextReviewDate!.Value));

        var merged = new List<Card>(capacity: Math.Min(limit, all.Count));
        foreach (var c in due)
        {
            if (merged.Count >= limit)
                break;
            merged.Add(c);
        }

        foreach (var c in upcoming)
        {
            if (merged.Count >= limit)
                break;
            merged.Add(c);
        }

        var dueCount = merged.Count(c => c.NextReviewDate is null || c.NextReviewDate <= nowUtc);
        var upcomingCount = merged.Count - dueCount;

        return (merged, dueCount, upcomingCount);
    }

    private static int CompareDue(Card a, Card b)
    {
        var aNull = a.NextReviewDate is null;
        var bNull = b.NextReviewDate is null;
        if (aNull && !bNull)
            return -1;
        if (!aNull && bNull)
            return 1;
        if (aNull && bNull)
            return string.Compare(a.Id.Value.ToString(), b.Id.Value.ToString(), StringComparison.Ordinal);
        return a.NextReviewDate!.Value.CompareTo(b.NextReviewDate!.Value);
    }
}
