namespace Flashcards.Application.DeckTags.CreateDeckTag;

public record CreateDeckTagCommand(string DeckId, string UserId, string Name);
