using Flashcards.Domain.Decks;

namespace Flashcards.Application.Decks;

public interface IDeckRepository
{
    Task SaveAsync(Deck deck, CancellationToken cancellationToken = default);

    Task<Deck?> GetByIdAsync(string deckId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Deck> Decks, string? NextPaginationToken)> GetByUserIdAsync(
        string userId,
        int? pageSize = null,
        string? paginationToken = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string deckId, CancellationToken cancellationToken = default);
}
