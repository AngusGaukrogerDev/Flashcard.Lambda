using Flashcards.Domain.Cards;
using Shouldly;

namespace Flashcards.Domain.Tests.Cards;

public class CardSchedulingTests
{
    private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CalculateNext_Incorrect_SetsRelearnAndResetsReps()
    {
        var (ease, interval, reps, next) = CardScheduling.CalculateNext(
            RecallRating.Incorrect,
            Now,
            CardScheduling.DefaultEaseFactor,
            5,
            3);

        reps.ShouldBe(0);
        interval.ShouldBe(0);
        next.ShouldBe(Now.AddMinutes(CardScheduling.RelearnMinutes));
        ease.ShouldBeLessThan(CardScheduling.DefaultEaseFactor);
    }

    [Theory]
    [InlineData(RecallRating.Hard, 0.5)]
    [InlineData(RecallRating.Medium, 1.0)]
    [InlineData(RecallRating.Easy, 2.0)]
    public void CalculateNext_FirstSuccess_UsesGraduatedFirstInterval(RecallRating rating, double expectedDays)
    {
        var (_, interval, reps, next) = CardScheduling.CalculateNext(
            rating,
            Now,
            CardScheduling.DefaultEaseFactor,
            0,
            0);

        reps.ShouldBe(1);
        interval.ShouldBe(expectedDays);
        next.ShouldBe(Now.AddDays(expectedDays));
    }

    [Fact]
    public void CalculateNext_SecondReview_ScalesByEaseAndRating()
    {
        var (ease, interval, reps, _) = CardScheduling.CalculateNext(
            RecallRating.Medium,
            Now,
            CardScheduling.DefaultEaseFactor,
            1.0,
            1);

        reps.ShouldBe(2);
        interval.ShouldBe(1.0 * CardScheduling.DefaultEaseFactor);
        ease.ShouldBe(CardScheduling.DefaultEaseFactor);
    }
}
