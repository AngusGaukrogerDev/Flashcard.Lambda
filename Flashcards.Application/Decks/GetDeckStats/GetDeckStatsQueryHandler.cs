using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Cards;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.Decks.GetDeckStats;

public class GetDeckStatsQueryHandler : IQueryHandler<GetDeckStatsQuery, GetDeckStatsResponse>
{
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly ICardReadRepository _cardReadRepository;

    public GetDeckStatsQueryHandler(IDeckReadRepository deckReadRepository, ICardReadRepository cardReadRepository)
    {
        _deckReadRepository = deckReadRepository;
        _cardReadRepository = cardReadRepository;
    }

    public GetDeckStatsQueryHandler(IDeckRepository deckRepository, ICardRepository cardRepository)
        : this((IDeckReadRepository)deckRepository, (ICardReadRepository)cardRepository)
    {
    }

    public async Task<GetDeckStatsResponse> HandleAsync(
        GetDeckStatsQuery query,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(query.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(query.DeckId);

        if (deck.UserId.Value != query.UserId)
            throw new UnauthorisedDeckAccessException(query.DeckId);

        var cards = await _cardReadRepository.GetAllByDeckIdAsync(query.DeckId, cancellationToken);

        var newCount = 0;
        var incorrectCount = 0;
        var hardCount = 0;
        var mediumCount = 0;
        var easyCount = 0;

        foreach (var card in cards)
        {
            switch (card.LastRecallRating)
            {
                case null:
                    newCount++;
                    break;
                case RecallRating.Incorrect:
                    incorrectCount++;
                    break;
                case RecallRating.Hard:
                    hardCount++;
                    break;
                case RecallRating.Medium:
                    mediumCount++;
                    break;
                case RecallRating.Easy:
                    easyCount++;
                    break;
                default:
                    newCount++;
                    break;
            }
        }

        return new GetDeckStatsResponse(
            query.DeckId,
            cards.Count,
            newCount,
            incorrectCount,
            hardCount,
            mediumCount,
            easyCount);
    }
}

