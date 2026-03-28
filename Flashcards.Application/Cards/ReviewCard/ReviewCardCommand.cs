using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.ReviewCard;

public record ReviewCardCommand(string CardId, string UserId, RecallRating Rating);
