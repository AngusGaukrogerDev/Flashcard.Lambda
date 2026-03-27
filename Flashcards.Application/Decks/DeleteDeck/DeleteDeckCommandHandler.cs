using Flashcards.Domain.Decks;

namespace Flashcards.Application.Decks.DeleteDeck;

public class DeleteDeckCommandHandler
{
    private readonly IDeckRepository _deckRepository;

    public DeleteDeckCommandHandler(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository;
    }

    public async Task HandleAsync(
        DeleteDeckCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        await _deckRepository.DeleteAsync(command.DeckId, cancellationToken);
    }
}
