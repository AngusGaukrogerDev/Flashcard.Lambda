namespace Flashcards.Application.DeckTags.DeleteDeckTag;

public record DeleteDeckTagCommand(string DeckId, string TagId, string UserId);
