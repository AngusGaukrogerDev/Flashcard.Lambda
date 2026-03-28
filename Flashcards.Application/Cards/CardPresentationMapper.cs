using Flashcards.Application.Cards.AddCardToDeck;
using Flashcards.Application.Cards.GetCardById;
using Flashcards.Application.Cards.GetCardsByDeck;
using Flashcards.Application.Cards.GetCardsForStudy;
using Flashcards.Application.Cards.UpdateCard;
using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards;

public static class CardPresentationMapper
{
    public static CardSummary ToSummary(Card card) => new(
        card.Id.Value,
        card.FrontText,
        card.BackText,
        card.CreatedAt,
        card.NextReviewDate,
        card.EaseFactor,
        card.IntervalDays,
        card.RepetitionCount,
        card.LastReviewedAt,
        card.LastRecallRating,
        RecallTrafficLightMapper.FromLastRecallRating(card.LastRecallRating),
        card.FrontPrompt,
        card.BackPrompt,
        card.BackgroundColour,
        card.TextColour,
        card.TagIds);

    public static GetCardByIdResponse ToGetCardByIdResponse(Card card) => new(
        card.Id.Value,
        card.FrontText,
        card.BackText,
        card.DeckId,
        card.CreatedAt,
        card.NextReviewDate,
        card.EaseFactor,
        card.IntervalDays,
        card.RepetitionCount,
        card.LastReviewedAt,
        card.LastRecallRating,
        RecallTrafficLightMapper.FromLastRecallRating(card.LastRecallRating),
        card.FrontPrompt,
        card.BackPrompt,
        card.BackgroundColour,
        card.TextColour,
        card.TagIds);

    public static UpdateCardResponse ToUpdateCardResponse(Card card) => new(
        card.Id.Value,
        card.FrontText,
        card.BackText,
        card.DeckId,
        card.CreatedAt,
        card.NextReviewDate,
        card.EaseFactor,
        card.IntervalDays,
        card.RepetitionCount,
        card.LastReviewedAt,
        card.LastRecallRating,
        RecallTrafficLightMapper.FromLastRecallRating(card.LastRecallRating),
        card.FrontPrompt,
        card.BackPrompt,
        card.BackgroundColour,
        card.TextColour,
        card.TagIds);

    public static AddCardToDeckResponse ToAddCardToDeckResponse(Card card) => new(
        card.Id.Value,
        card.FrontText,
        card.BackText,
        card.DeckId,
        card.CreatedAt,
        card.NextReviewDate,
        card.EaseFactor,
        card.IntervalDays,
        card.RepetitionCount,
        card.LastReviewedAt,
        card.LastRecallRating,
        RecallTrafficLightMapper.FromLastRecallRating(card.LastRecallRating),
        card.FrontPrompt,
        card.BackPrompt,
        card.BackgroundColour,
        card.TextColour,
        card.TagIds);

    public static StudyCardItem ToStudyCardItem(Card card, bool isDue)
    {
        var s = ToSummary(card);
        return new StudyCardItem(
            s.Id,
            s.FrontText,
            s.BackText,
            s.CreatedAt,
            s.NextReviewDate,
            s.EaseFactor,
            s.IntervalDays,
            s.RepetitionCount,
            s.LastReviewedAt,
            s.LastRecallRating,
            s.RecallTrafficLight,
            s.FrontPrompt,
            s.BackPrompt,
            s.BackgroundColour,
            s.TextColour,
            s.TagIds,
            isDue);
    }
}
