namespace Flashcards.Application.DeckTags.UpdateDeckTag;

public record UpdateDeckTagResponse(Guid Id, string DeckId, string Name, DateTime CreatedAt);
