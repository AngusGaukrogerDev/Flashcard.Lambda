using Flashcards.Application.Decks;

namespace Flashcards.Application.Decks.GetDecks;

public class GetDecksQueryHandler
{
    private readonly IDeckRepository _deckRepository;

    public GetDecksQueryHandler(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository;
    }

    public async Task<GetDecksResponse> HandleAsync(
        GetDecksQuery query,
        CancellationToken cancellationToken = default)
    {
        var (decks, nextToken) = await _deckRepository.GetByUserIdAsync(
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
