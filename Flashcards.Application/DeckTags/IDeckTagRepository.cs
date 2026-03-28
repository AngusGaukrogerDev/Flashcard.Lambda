using Flashcards.Domain.DeckTags;

namespace Flashcards.Application.DeckTags;

public interface IDeckTagWriteRepository
{
    Task SaveAsync(DeckTag tag, CancellationToken cancellationToken = default);

    Task DeleteAsync(string tagId, CancellationToken cancellationToken = default);

    Task DeleteAllForDeckAsync(string deckId, CancellationToken cancellationToken = default);
}

public interface IDeckTagReadRepository
{
    Task<DeckTag?> GetByIdAsync(string tagId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeckTag>> GetByDeckIdAsync(string deckId, CancellationToken cancellationToken = default);
}

public interface IDeckTagRepository : IDeckTagReadRepository, IDeckTagWriteRepository;
