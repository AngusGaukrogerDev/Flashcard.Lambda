using Flashcards.Domain.Cards;
using Flashcards.Application.Abstractions.Commands;

namespace Flashcards.Application.Cards.UpdateCard;

public class UpdateCardCommandHandler : ICommandHandler<UpdateCardCommand, UpdateCardResponse>
{
    private readonly ICardReadRepository _cardReadRepository;
    private readonly ICardWriteRepository _cardWriteRepository;

    public UpdateCardCommandHandler(ICardReadRepository cardReadRepository, ICardWriteRepository cardWriteRepository)
    {
        _cardReadRepository = cardReadRepository;
        _cardWriteRepository = cardWriteRepository;
    }

    public UpdateCardCommandHandler(ICardRepository cardRepository)
        : this(cardRepository, cardRepository)
    {
    }

    public async Task<UpdateCardResponse> HandleAsync(
        UpdateCardCommand command,
        CancellationToken cancellationToken = default)
    {
        var card = await _cardReadRepository.GetByIdAsync(command.CardId, cancellationToken)
            ?? throw new CardNotFoundException(command.CardId);

        if (card.UserId.Value != command.UserId)
            throw new UnauthorisedCardAccessException(command.CardId);

        card.Update(
            command.FrontText,
            command.BackText,
            command.FrontPrompt,
            command.BackPrompt,
            command.BackgroundColour,
            command.TextColour);

        await _cardWriteRepository.SaveAsync(card, cancellationToken);

        return new UpdateCardResponse(
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
