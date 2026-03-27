using Flashcards.Domain.Decks;

namespace Flashcards.Application.Decks.CreateDeck;

public class CreateDeckCommandHandler
{
    private readonly IDeckRepository _deckRepository;

    public CreateDeckCommandHandler(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository;
    }

    public async Task<CreateDeckResponse> HandleAsync(
        CreateDeckCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = Deck.Create(command.Name, command.Description);

        await _deckRepository.SaveAsync(deck, cancellationToken);

        return new CreateDeckResponse(deck.Id.Value, deck.Name, deck.CreatedAt);
    }
}
