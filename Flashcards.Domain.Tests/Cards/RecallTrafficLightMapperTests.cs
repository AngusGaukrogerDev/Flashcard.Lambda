using Flashcards.Domain.Cards;
using Shouldly;

namespace Flashcards.Domain.Tests.Cards;

public class RecallTrafficLightMapperTests
{
    [Theory]
    [InlineData(RecallRating.Incorrect, RecallTrafficLight.Red)]
    [InlineData(RecallRating.Hard, RecallTrafficLight.Orange)]
    [InlineData(RecallRating.Medium, RecallTrafficLight.Yellow)]
    [InlineData(RecallRating.Easy, RecallTrafficLight.Green)]
    public void FromLastRecallRating_MapsRatings(RecallRating rating, RecallTrafficLight expected)
    {
        RecallTrafficLightMapper.FromLastRecallRating(rating).ShouldBe(expected);
    }

    [Fact]
    public void FromLastRecallRating_WhenNull_ReturnsNull()
    {
        RecallTrafficLightMapper.FromLastRecallRating(null).ShouldBeNull();
    }
}
