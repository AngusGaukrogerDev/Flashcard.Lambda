using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Cards;
using Flashcards.Application.Decks;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.Cards.GetCardsForStudy;

public class GetCardsForStudyQueryHandler : IQueryHandler<GetCardsForStudyQuery, GetCardsForStudyResponse>
{
    private readonly ICardReadRepository _cardReadRepository;
    private readonly IDeckReadRepository _deckReadRepository;

    public GetCardsForStudyQueryHandler(ICardReadRepository cardReadRepository, IDeckReadRepository deckReadRepository)
    {
        _cardReadRepository = cardReadRepository;
        _deckReadRepository = deckReadRepository;
    }

    public GetCardsForStudyQueryHandler(ICardRepository cardRepository, IDeckRepository deckRepository)
        : this((ICardReadRepository)cardRepository, (IDeckReadRepository)deckRepository)
    {
    }

    public async Task<GetCardsForStudyResponse> HandleAsync(
        GetCardsForStudyQuery query,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(query.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(query.DeckId);

        if (deck.UserId.Value != query.UserId)
            throw new UnauthorisedDeckAccessException(query.DeckId);

        var allCards = await _cardReadRepository.GetAllByDeckIdAsync(query.DeckId, cancellationToken);
        var now = DateTime.UtcNow;
        var limit = query.Limit <= 0 ? 20 : query.Limit;
        var (ordered, dueCount, upcomingCount) = StudyQueueOrdering.SelectCards(allCards, now, limit);

        var items = ordered
            .Select(c =>
            {
                var isDue = c.NextReviewDate is null || c.NextReviewDate <= now;
                return CardPresentationMapper.ToStudyCardItem(c, isDue);
            })
            .ToList();

        return new GetCardsForStudyResponse(query.DeckId, items, dueCount, upcomingCount);
    }
}
