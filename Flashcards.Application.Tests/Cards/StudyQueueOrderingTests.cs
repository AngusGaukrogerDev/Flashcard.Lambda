using Flashcards.Application.Cards;
using Flashcards.Domain.Cards;
using Shouldly;

namespace Flashcards.Application.Tests.Cards;

public class StudyQueueOrderingTests
{
    private const string UserId = "user-1";
    private const string DeckId = "deck-1";

    [Fact]
    public void SelectCards_PrefersDueBeforeUpcoming()
    {
        var now = new DateTime(2025, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        var future = now.AddDays(5);
        var due = Card.Reconstitute(CardId.New(), "a", "b", DeckId, UserId, DateTime.UtcNow, now.AddDays(-1));
        var upcoming = Card.Reconstitute(CardId.New(), "c", "d", DeckId, UserId, DateTime.UtcNow, future);

        var (cards, dueCount, upcomingCount) = StudyQueueOrdering.SelectCards(
            new[] { upcoming, due },
            now,
            limit: 10);

        cards[0].Id.ShouldBe(due.Id);
        cards[1].Id.ShouldBe(upcoming.Id);
        dueCount.ShouldBe(1);
        upcomingCount.ShouldBe(1);
    }

    [Fact]
    public void SelectCards_FillsWithUpcomingWhenFewDue()
    {
        var now = new DateTime(2025, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        var due = Card.Reconstitute(CardId.New(), "a", "b", DeckId, UserId, DateTime.UtcNow, null);
        var up1 = Card.Reconstitute(CardId.New(), "c", "d", DeckId, UserId, DateTime.UtcNow, now.AddDays(1));
        var up2 = Card.Reconstitute(CardId.New(), "e", "f", DeckId, UserId, DateTime.UtcNow, now.AddDays(2));

        var (cards, dueCount, upcomingCount) = StudyQueueOrdering.SelectCards(
            new[] { up2, due, up1 },
            now,
            limit: 3);

        cards.Count.ShouldBe(3);
        dueCount.ShouldBe(1);
        upcomingCount.ShouldBe(2);
        cards[0].Id.ShouldBe(due.Id);
    }

    [Fact]
    public void SelectCards_RespectsMaxLimit()
    {
        var now = DateTime.UtcNow;
        var cards = Enumerable.Range(0, 60)
            .Select(_ => Card.Reconstitute(CardId.New(), "a", "b", DeckId, UserId, DateTime.UtcNow, null))
            .ToList();

        var (selected, _, _) = StudyQueueOrdering.SelectCards(cards, now, StudyQueueOrdering.MaxLimit + 100);

        selected.Count.ShouldBe(StudyQueueOrdering.MaxLimit);
    }
}
