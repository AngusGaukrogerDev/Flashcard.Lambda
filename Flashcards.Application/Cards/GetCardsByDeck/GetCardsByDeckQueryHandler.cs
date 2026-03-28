using Flashcards.Application.Decks;
using Flashcards.Application.Abstractions.Queries;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.Cards.GetCardsByDeck;

public class GetCardsByDeckQueryHandler : IQueryHandler<GetCardsByDeckQuery, GetCardsByDeckResponse>
{
    private readonly ICardReadRepository _cardReadRepository;
    private readonly IDeckReadRepository _deckReadRepository;

    public GetCardsByDeckQueryHandler(ICardReadRepository cardReadRepository, IDeckReadRepository deckReadRepository)
    {
        _cardReadRepository = cardReadRepository;
        _deckReadRepository = deckReadRepository;
    }

    public GetCardsByDeckQueryHandler(ICardRepository cardRepository, IDeckRepository deckRepository)
        : this((ICardReadRepository)cardRepository, (IDeckReadRepository)deckRepository)
    {
    }

    public async Task<GetCardsByDeckResponse> HandleAsync(
        GetCardsByDeckQuery query,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(query.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(query.DeckId);

        if (deck.UserId.Value != query.UserId)
            throw new UnauthorisedDeckAccessException(query.DeckId);

        IReadOnlyList<Domain.Cards.Card> cards;
        string? nextPaginationToken = null;

        if (query.PageSize.HasValue || query.PaginationToken is not null)
        {
            var paged = await _cardReadRepository.GetByDeckIdAsync(
                query.DeckId,
                query.PageSize,
                query.PaginationToken,
                cancellationToken);
            cards = paged.Cards;
            nextPaginationToken = paged.NextPaginationToken;
        }
        else
        {
            cards = await _cardReadRepository.GetByDeckIdAsync(query.DeckId, cancellationToken);
        }

        var summaries = cards
            .Select(c => new CardSummary(c.Id.Value, c.FrontText, c.BackText, c.CreatedAt, c.NextReviewDate, c.FrontPrompt, c.BackPrompt, c.BackgroundColour, c.TextColour, c.TagIds))
            .ToList();

        return new GetCardsByDeckResponse(query.DeckId, summaries, nextPaginationToken);
    }
}
