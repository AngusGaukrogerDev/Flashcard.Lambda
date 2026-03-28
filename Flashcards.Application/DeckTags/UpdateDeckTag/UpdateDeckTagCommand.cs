namespace Flashcards.Application.DeckTags.UpdateDeckTag;

public record UpdateDeckTagCommand(string DeckId, string TagId, string UserId, string Name);
