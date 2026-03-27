using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.GetCardById;

public class GetCardByIdQueryHandler
{
    private readonly ICardRepository _cardRepository;

    public GetCardByIdQueryHandler(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public async Task<GetCardByIdResponse> HandleAsync(
        GetCardByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(query.CardId, cancellationToken)
            ?? throw new CardNotFoundException(query.CardId);

        if (card.UserId != query.UserId)
            throw new UnauthorisedCardAccessException(query.CardId);

        return new GetCardByIdResponse(
            card.Id.Value,
            card.FrontText,
            card.BackText,
            card.DeckId,
            card.CreatedAt,
            card.NextReviewDate,
            card.FrontPrompt,
            card.BackPrompt,
            card.BackgroundColour,
            card.TextColour);
    }
}
