using Flashcards.Domain.Decks;

namespace Flashcards.Application.Decks.UpdateDeck;

public class UpdateDeckCommandHandler
{
    private readonly IDeckRepository _deckRepository;

    public UpdateDeckCommandHandler(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository;
    }

    public async Task<UpdateDeckResponse> HandleAsync(
        UpdateDeckCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        deck.Update(command.Name, command.Description);

        await _deckRepository.SaveAsync(deck, cancellationToken);

        return new UpdateDeckResponse(deck.Id.Value, deck.Name, deck.Description, deck.CreatedAt);
    }
}
