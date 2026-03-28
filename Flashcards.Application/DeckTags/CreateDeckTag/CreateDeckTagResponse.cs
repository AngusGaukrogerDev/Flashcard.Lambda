namespace Flashcards.Application.DeckTags.CreateDeckTag;

public record CreateDeckTagResponse(Guid Id, string DeckId, string Name, DateTime CreatedAt);
