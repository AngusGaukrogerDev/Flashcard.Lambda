using Flashcards.Domain.Decks;

namespace Flashcards.Application.Decks;

public interface IDeckRepository
{
    Task SaveAsync(Deck deck, CancellationToken cancellationToken = default);
}
