using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards;

public interface ICardRepository
{
    Task SaveAsync(Card card, CancellationToken cancellationToken = default);

    Task<Card?> GetByIdAsync(string cardId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Card>> GetByDeckIdAsync(string deckId, CancellationToken cancellationToken = default);

    Task DeleteAsync(string cardId, CancellationToken cancellationToken = default);
}
