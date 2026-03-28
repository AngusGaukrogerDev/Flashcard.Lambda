using Flashcards.Domain.Cards;
using Shouldly;

namespace Flashcards.Domain.Tests.Cards;

public class CardApplyRecallRatingTests
{
    private const string DeckId = "deck-1";
    private const string UserId = "user-1";

    [Fact]
    public void ApplyRecallRating_SetsLastRecallRatingAndNextReview()
    {
        var card = Card.Create("a", "b", DeckId, UserId);
        var reviewedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        card.ApplyRecallRating(RecallRating.Medium, reviewedAt);

        card.LastRecallRating.ShouldBe(RecallRating.Medium);
        card.LastReviewedAt.ShouldBe(reviewedAt);
        card.NextReviewDate.ShouldNotBeNull();
        card.RepetitionCount.ShouldBe(1);
    }

    [Fact]
    public void ApplyRecallRating_ConvertsLocalTimeToUtc()
    {
        var card = Card.Create("a", "b", DeckId, UserId);
        var local = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Local);

        card.ApplyRecallRating(RecallRating.Easy, local);

        card.LastReviewedAt!.Value.Kind.ShouldBe(DateTimeKind.Utc);
    }
}
