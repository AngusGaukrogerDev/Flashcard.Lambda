using Flashcards.Domain.Decks;
using Flashcards.Application.Abstractions.Commands;

namespace Flashcards.Application.Decks.DeleteDeck;

public class DeleteDeckCommandHandler : ICommandHandler<DeleteDeckCommand>
{
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly IDeckWriteRepository _deckWriteRepository;

    public DeleteDeckCommandHandler(IDeckReadRepository deckReadRepository, IDeckWriteRepository deckWriteRepository)
    {
        _deckReadRepository = deckReadRepository;
        _deckWriteRepository = deckWriteRepository;
    }

    public DeleteDeckCommandHandler(IDeckRepository deckRepository)
        : this(deckRepository, deckRepository)
    {
    }

    public async Task HandleAsync(
        DeleteDeckCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId.Value != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        await _deckWriteRepository.DeleteAsync(command.DeckId, cancellationToken);
    }
}
