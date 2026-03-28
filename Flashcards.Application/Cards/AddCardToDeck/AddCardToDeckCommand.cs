using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.AddCardToDeck;

public record AddCardToDeckCommand(
    string FrontText,
    string BackText,
    string DeckId,
    string UserId,
    string? FrontPrompt = null,
    string? BackPrompt = null,
    CardColour? BackgroundColour = null,
    TextColour? TextColour = null,
    IReadOnlyList<string>? TagIds = null);
