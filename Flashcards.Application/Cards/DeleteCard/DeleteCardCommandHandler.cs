using Flashcards.Domain.Cards;
using Flashcards.Application.Abstractions.Commands;

namespace Flashcards.Application.Cards.DeleteCard;

public class DeleteCardCommandHandler : ICommandHandler<DeleteCardCommand>
{
    private readonly ICardReadRepository _cardReadRepository;
    private readonly ICardWriteRepository _cardWriteRepository;

    public DeleteCardCommandHandler(ICardReadRepository cardReadRepository, ICardWriteRepository cardWriteRepository)
    {
        _cardReadRepository = cardReadRepository;
        _cardWriteRepository = cardWriteRepository;
    }

    public DeleteCardCommandHandler(ICardRepository cardRepository)
        : this(cardRepository, cardRepository)
    {
    }

    public async Task HandleAsync(
        DeleteCardCommand command,
        CancellationToken cancellationToken = default)
    {
        var card = await _cardReadRepository.GetByIdAsync(command.CardId, cancellationToken)
            ?? throw new CardNotFoundException(command.CardId);

        if (card.UserId.Value != command.UserId)
            throw new UnauthorisedCardAccessException(command.CardId);

        await _cardWriteRepository.DeleteAsync(command.CardId, cancellationToken);
    }
}
