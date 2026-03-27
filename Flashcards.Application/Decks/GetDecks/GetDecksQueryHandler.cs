using Flashcards.Application.Decks;
using Flashcards.Application.Abstractions.Queries;

namespace Flashcards.Application.Decks.GetDecks;

public class GetDecksQueryHandler : IQueryHandler<GetDecksQuery, GetDecksResponse>
{
    private readonly IDeckReadRepository _deckReadRepository;

    public GetDecksQueryHandler(IDeckReadRepository deckReadRepository)
    {
        _deckReadRepository = deckReadRepository;
    }

    public async Task<GetDecksResponse> HandleAsync(
        GetDecksQuery query,
        CancellationToken cancellationToken = default)
    {
        var (decks, nextToken) = await _deckReadRepository.GetByUserIdAsync(
            query.UserId,
            query.PageSize,
            query.PaginationToken,
            cancellationToken);

        var summaries = decks
            .Select(d => new DeckSummary(d.Id.Value, d.Name, d.Description, d.CreatedAt))
            .ToList();

        return new GetDecksResponse(summaries, nextToken);
    }
}
