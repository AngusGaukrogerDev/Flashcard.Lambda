using Flashcards.Domain.Decks;
using Flashcards.Application.Abstractions.Commands;

namespace Flashcards.Application.Decks.UpdateDeck;

public class UpdateDeckCommandHandler : ICommandHandler<UpdateDeckCommand, UpdateDeckResponse>
{
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly IDeckWriteRepository _deckWriteRepository;

    public UpdateDeckCommandHandler(IDeckReadRepository deckReadRepository, IDeckWriteRepository deckWriteRepository)
    {
        _deckReadRepository = deckReadRepository;
        _deckWriteRepository = deckWriteRepository;
    }

    public UpdateDeckCommandHandler(IDeckRepository deckRepository)
        : this(deckRepository, deckRepository)
    {
    }

    public async Task<UpdateDeckResponse> HandleAsync(
        UpdateDeckCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId.Value != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        deck.Update(command.Name, command.Description);

        await _deckWriteRepository.SaveAsync(deck, cancellationToken);

        return new UpdateDeckResponse(deck.Id.Value, deck.Name, deck.Description, deck.CreatedAt);
    }
}
