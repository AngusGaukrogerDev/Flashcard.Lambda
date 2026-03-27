using Flashcards.Application.Decks;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.Cards.AddCardToDeck;

public class AddCardToDeckCommandHandler
{
    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;

    public AddCardToDeckCommandHandler(ICardRepository cardRepository, IDeckRepository deckRepository)
    {
        _cardRepository = cardRepository;
        _deckRepository = deckRepository;
    }

    public async Task<AddCardToDeckResponse> HandleAsync(
        AddCardToDeckCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        var card = Card.Create(
            command.FrontText,
            command.BackText,
            command.DeckId,
            command.UserId,
            command.FrontPrompt,
            command.BackPrompt,
            command.BackgroundColour,
            command.TextColour);

        await _cardRepository.SaveAsync(card, cancellationToken);

        return new AddCardToDeckResponse(
            card.Id.Value,
            card.FrontText,
            card.BackText,
            card.DeckId,
            card.CreatedAt,
            card.FrontPrompt,
            card.BackPrompt,
            card.BackgroundColour,
            card.TextColour);
    }
}
