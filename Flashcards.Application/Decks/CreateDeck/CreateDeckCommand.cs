namespace Flashcards.Application.Decks.CreateDeck;

public record CreateDeckCommand(string Name, string? Description, string UserId);
