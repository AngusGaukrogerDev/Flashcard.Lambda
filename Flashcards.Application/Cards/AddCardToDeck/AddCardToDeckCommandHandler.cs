using Flashcards.Application.Decks;
using Flashcards.Application.DeckTags;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.Cards.AddCardToDeck;

public class AddCardToDeckCommandHandler : ICommandHandler<AddCardToDeckCommand, AddCardToDeckResponse>
{
    private readonly ICardWriteRepository _cardWriteRepository;
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly IDeckTagReadRepository _deckTagReadRepository;

    public AddCardToDeckCommandHandler(
        ICardWriteRepository cardWriteRepository,
        IDeckReadRepository deckReadRepository,
        IDeckTagReadRepository deckTagReadRepository)
    {
        _cardWriteRepository = cardWriteRepository;
        _deckReadRepository = deckReadRepository;
        _deckTagReadRepository = deckTagReadRepository;
    }

    public AddCardToDeckCommandHandler(ICardRepository cardRepository, IDeckRepository deckRepository, IDeckTagReadRepository deckTagReadRepository)
        : this((ICardWriteRepository)cardRepository, (IDeckReadRepository)deckRepository, deckTagReadRepository)
    {
    }

    public async Task<AddCardToDeckResponse> HandleAsync(
        AddCardToDeckCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId.Value != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        if (command.TagIds is { Count: > 0 })
        {
            var deckTags = await _deckTagReadRepository.GetByDeckIdAsync(command.DeckId, cancellationToken);
            DeckTagCardGuard.EnsureTagIdsBelongToDeck(command.DeckId, command.TagIds, deckTags);
        }

        var card = Card.Create(
            command.FrontText,
            command.BackText,
            command.DeckId,
            command.UserId,
            command.FrontPrompt,
            command.BackPrompt,
            command.BackgroundColour,
            command.TextColour,
            command.TagIds);

        await _cardWriteRepository.SaveAsync(card, cancellationToken);

        return new AddCardToDeckResponse(
            card.Id.Value,
            card.FrontText,
            card.BackText,
            card.DeckId,
            card.CreatedAt,
            card.FrontPrompt,
            card.BackPrompt,
            card.BackgroundColour,
            card.TextColour,
            card.TagIds);
    }
}
