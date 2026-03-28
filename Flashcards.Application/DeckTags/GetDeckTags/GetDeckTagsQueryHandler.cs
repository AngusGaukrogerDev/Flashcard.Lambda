using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Decks;
using Flashcards.Application.DeckTags;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.DeckTags.GetDeckTags;

public class GetDeckTagsQueryHandler : IQueryHandler<GetDeckTagsQuery, GetDeckTagsResponse>
{
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly IDeckTagReadRepository _deckTagReadRepository;

    public GetDeckTagsQueryHandler(IDeckReadRepository deckReadRepository, IDeckTagReadRepository deckTagReadRepository)
    {
        _deckReadRepository = deckReadRepository;
        _deckTagReadRepository = deckTagReadRepository;
    }

    public GetDeckTagsQueryHandler(IDeckRepository deckRepository, IDeckTagReadRepository deckTagReadRepository)
        : this((IDeckReadRepository)deckRepository, deckTagReadRepository)
    {
    }

    public async Task<GetDeckTagsResponse> HandleAsync(
        GetDeckTagsQuery query,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(query.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(query.DeckId);

        if (deck.UserId.Value != query.UserId)
            throw new UnauthorisedDeckAccessException(query.DeckId);

        var tags = await _deckTagReadRepository.GetByDeckIdAsync(query.DeckId, cancellationToken);
        var ordered = tags
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(t => new DeckTagSummary(t.Id.Value, t.DeckId, t.Name, t.CreatedAt))
            .ToList();

        return new GetDeckTagsResponse(ordered);
    }
}
