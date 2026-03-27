using Flashcards.Domain.Decks;
using Flashcards.Application.Abstractions.Commands;

namespace Flashcards.Application.Decks.CreateDeck;

public class CreateDeckCommandHandler : ICommandHandler<CreateDeckCommand, CreateDeckResponse>
{
    private readonly IDeckWriteRepository _deckWriteRepository;

    public CreateDeckCommandHandler(IDeckWriteRepository deckWriteRepository)
    {
        _deckWriteRepository = deckWriteRepository;
    }

    public async Task<CreateDeckResponse> HandleAsync(
        CreateDeckCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = Deck.Create(command.Name, command.UserId, command.Description);

        await _deckWriteRepository.SaveAsync(deck, cancellationToken);

        return new CreateDeckResponse(deck.Id.Value, deck.Name, deck.CreatedAt);
    }
}
