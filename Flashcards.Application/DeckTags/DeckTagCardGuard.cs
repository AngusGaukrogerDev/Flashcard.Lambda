using Flashcards.Domain.DeckTags;

namespace Flashcards.Application.DeckTags;

public static class DeckTagCardGuard
{
    public static void EnsureTagIdsBelongToDeck(string deckId, IReadOnlyList<string> tagIds, IReadOnlyList<DeckTag> tagsForDeck)
    {
        if (tagIds.Count == 0)
            return;

        var allowed = tagsForDeck.Select(t => t.Id.Value.ToString()).ToHashSet(StringComparer.Ordinal);
        foreach (var id in tagIds)
        {
            if (!allowed.Contains(id))
                throw new InvalidDeckTagForDeckException(id, deckId);
        }
    }
}
