namespace Flashcards.Domain.Cards;

public static class RecallTrafficLightMapper
{
    public static RecallTrafficLight? FromLastRecallRating(RecallRating? lastRecallRating)
        => lastRecallRating switch
        {
            RecallRating.Incorrect => RecallTrafficLight.Red,
            RecallRating.Hard => RecallTrafficLight.Orange,
            RecallRating.Medium => RecallTrafficLight.Yellow,
            RecallRating.Easy => RecallTrafficLight.Green,
            null => null,
            _ => null
        };
}
