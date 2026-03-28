using Flashcards.Application.Cards;
using Flashcards.Application.DeckTags;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.UpdateCard;

public class UpdateCardCommandHandler : ICommandHandler<UpdateCardCommand, UpdateCardResponse>
{
    private readonly ICardReadRepository _cardReadRepository;
    private readonly ICardWriteRepository _cardWriteRepository;
    private readonly IDeckTagReadRepository _deckTagReadRepository;

    public UpdateCardCommandHandler(
        ICardReadRepository cardReadRepository,
        ICardWriteRepository cardWriteRepository,
        IDeckTagReadRepository deckTagReadRepository)
    {
        _cardReadRepository = cardReadRepository;
        _cardWriteRepository = cardWriteRepository;
        _deckTagReadRepository = deckTagReadRepository;
    }

    public UpdateCardCommandHandler(ICardRepository cardRepository, IDeckTagReadRepository deckTagReadRepository)
        : this(cardRepository, cardRepository, deckTagReadRepository)
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

        if (command.TagIds is not null)
        {
            var deckTags = await _deckTagReadRepository.GetByDeckIdAsync(card.DeckId, cancellationToken);
            DeckTagCardGuard.EnsureTagIdsBelongToDeck(card.DeckId, command.TagIds, deckTags);
            card.SetTagIds(command.TagIds);
        }

        await _cardWriteRepository.SaveAsync(card, cancellationToken);

        return CardPresentationMapper.ToUpdateCardResponse(card);
    }
}
