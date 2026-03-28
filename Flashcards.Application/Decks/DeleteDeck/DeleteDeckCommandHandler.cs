using Flashcards.Domain.Decks;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.DeckTags;

namespace Flashcards.Application.Decks.DeleteDeck;

public class DeleteDeckCommandHandler : ICommandHandler<DeleteDeckCommand>
{
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly IDeckWriteRepository _deckWriteRepository;
    private readonly IDeckTagWriteRepository _deckTagWriteRepository;

    public DeleteDeckCommandHandler(
        IDeckReadRepository deckReadRepository,
        IDeckWriteRepository deckWriteRepository,
        IDeckTagWriteRepository deckTagWriteRepository)
    {
        _deckReadRepository = deckReadRepository;
        _deckWriteRepository = deckWriteRepository;
        _deckTagWriteRepository = deckTagWriteRepository;
    }

    public DeleteDeckCommandHandler(IDeckRepository deckRepository, IDeckTagWriteRepository deckTagWriteRepository)
        : this(deckRepository, deckRepository, deckTagWriteRepository)
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

        await _deckTagWriteRepository.DeleteAllForDeckAsync(command.DeckId, cancellationToken);
        await _deckWriteRepository.DeleteAsync(command.DeckId, cancellationToken);
    }
}
