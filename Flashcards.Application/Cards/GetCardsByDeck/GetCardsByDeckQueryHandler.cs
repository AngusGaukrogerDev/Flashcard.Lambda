using Flashcards.Application.Decks;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.Cards.GetCardsByDeck;

public class GetCardsByDeckQueryHandler
{
    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;

    public GetCardsByDeckQueryHandler(ICardRepository cardRepository, IDeckRepository deckRepository)
    {
        _cardRepository = cardRepository;
        _deckRepository = deckRepository;
    }

    public async Task<GetCardsByDeckResponse> HandleAsync(
        GetCardsByDeckQuery query,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckRepository.GetByIdAsync(query.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(query.DeckId);

        if (deck.UserId != query.UserId)
            throw new UnauthorisedDeckAccessException(query.DeckId);

        var cards = await _cardRepository.GetByDeckIdAsync(query.DeckId, cancellationToken);

        var summaries = cards
            .Select(c => new CardSummary(c.Id.Value, c.FrontText, c.BackText, c.CreatedAt, c.NextReviewDate, c.FrontPrompt, c.BackPrompt, c.BackgroundColour, c.TextColour))
            .ToList();

        return new GetCardsByDeckResponse(query.DeckId, summaries);
    }
}
