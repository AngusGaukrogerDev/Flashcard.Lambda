using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards;

public interface ICardWriteRepository
{
    Task SaveAsync(Card card, CancellationToken cancellationToken = default);

    Task DeleteAsync(string cardId, CancellationToken cancellationToken = default);
}

public interface ICardReadRepository
{
    Task<Card?> GetByIdAsync(string cardId, CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<Card>> GetByDeckIdAsync(string deckId, CancellationToken cancellationToken = default)
    {
        var (cards, _) = await GetByDeckIdAsync(deckId, null, null, cancellationToken);
        return cards;
    }

    Task<(IReadOnlyList<Card> Cards, string? NextPaginationToken)> GetByDeckIdAsync(
        string deckId,
        int? pageSize = null,
        string? paginationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads every card in the deck (paginates internally). Used for study queue ordering.
    /// </summary>
    Task<IReadOnlyList<Card>> GetAllByDeckIdAsync(string deckId, CancellationToken cancellationToken = default);
}

public interface ICardRepository : ICardReadRepository, ICardWriteRepository;
