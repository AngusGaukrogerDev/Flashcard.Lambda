using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.DeleteCard;

public class DeleteCardCommandHandler
{
    private readonly ICardRepository _cardRepository;

    public DeleteCardCommandHandler(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public async Task HandleAsync(
        DeleteCardCommand command,
        CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(command.CardId, cancellationToken)
            ?? throw new CardNotFoundException(command.CardId);

        if (card.UserId != command.UserId)
            throw new UnauthorisedCardAccessException(command.CardId);

        await _cardRepository.DeleteAsync(command.CardId, cancellationToken);
    }
}
