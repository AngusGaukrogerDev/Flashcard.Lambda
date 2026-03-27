using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.UpdateCard;

public class UpdateCardCommandHandler
{
    private readonly ICardRepository _cardRepository;

    public UpdateCardCommandHandler(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public async Task<UpdateCardResponse> HandleAsync(
        UpdateCardCommand command,
        CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(command.CardId, cancellationToken)
            ?? throw new CardNotFoundException(command.CardId);

        if (card.UserId != command.UserId)
            throw new UnauthorisedCardAccessException(command.CardId);

        card.Update(command.FrontText, command.BackText);

        await _cardRepository.SaveAsync(card, cancellationToken);

        return new UpdateCardResponse(
            card.Id.Value,
            card.FrontText,
            card.BackText,
            card.DeckId,
            card.CreatedAt,
            card.NextReviewDate);
    }
}
