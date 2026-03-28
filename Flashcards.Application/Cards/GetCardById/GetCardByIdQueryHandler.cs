using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Cards;
using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.GetCardById;

public class GetCardByIdQueryHandler : IQueryHandler<GetCardByIdQuery, GetCardByIdResponse>
{
    private readonly ICardReadRepository _cardReadRepository;

    public GetCardByIdQueryHandler(ICardReadRepository cardReadRepository)
    {
        _cardReadRepository = cardReadRepository;
    }

    public async Task<GetCardByIdResponse> HandleAsync(
        GetCardByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var card = await _cardReadRepository.GetByIdAsync(query.CardId, cancellationToken)
            ?? throw new CardNotFoundException(query.CardId);

        if (card.UserId.Value != query.UserId)
            throw new UnauthorisedCardAccessException(query.CardId);

        return CardPresentationMapper.ToGetCardByIdResponse(card);
    }
}
