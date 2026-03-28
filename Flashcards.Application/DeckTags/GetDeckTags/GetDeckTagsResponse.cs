namespace Flashcards.Application.DeckTags.GetDeckTags;

public record GetDeckTagsResponse(IReadOnlyList<DeckTagSummary> Tags);
