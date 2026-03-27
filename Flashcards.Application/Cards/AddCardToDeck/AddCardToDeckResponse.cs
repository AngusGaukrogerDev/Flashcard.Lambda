using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.AddCardToDeck;

public record AddCardToDeckResponse(
    Guid Id,
    string FrontText,
    string BackText,
    string DeckId,
    DateTime CreatedAt,
    string? FrontPrompt,
    string? BackPrompt,
    CardColour? BackgroundColour,
    TextColour? TextColour);
