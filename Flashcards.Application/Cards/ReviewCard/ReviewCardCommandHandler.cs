using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Cards.GetCardById;
using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.ReviewCard;

public class ReviewCardCommandHandler : ICommandHandler<ReviewCardCommand, GetCardByIdResponse>
{
    private readonly ICardReadRepository _cardReadRepository;
    private readonly ICardWriteRepository _cardWriteRepository;

    public ReviewCardCommandHandler(ICardReadRepository cardReadRepository, ICardWriteRepository cardWriteRepository)
    {
        _cardReadRepository = cardReadRepository;
        _cardWriteRepository = cardWriteRepository;
    }

    public ReviewCardCommandHandler(ICardRepository cardRepository)
        : this(cardRepository, cardRepository)
    {
    }

    public async Task<GetCardByIdResponse> HandleAsync(
        ReviewCardCommand command,
        CancellationToken cancellationToken = default)
    {
        var card = await _cardReadRepository.GetByIdAsync(command.CardId, cancellationToken)
            ?? throw new CardNotFoundException(command.CardId);

        if (card.UserId.Value != command.UserId)
            throw new UnauthorisedCardAccessException(command.CardId);

        card.ApplyRecallRating(command.Rating, DateTime.UtcNow);

        await _cardWriteRepository.SaveAsync(card, cancellationToken);

        return CardPresentationMapper.ToGetCardByIdResponse(card);
    }
}
