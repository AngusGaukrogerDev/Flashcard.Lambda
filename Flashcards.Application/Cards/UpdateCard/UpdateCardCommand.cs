using Flashcards.Domain.Cards;

namespace Flashcards.Application.Cards.UpdateCard;

public record UpdateCardCommand(
    string CardId,
    string UserId,
    string FrontText,
    string BackText,
    string? FrontPrompt = null,
    string? BackPrompt = null,
    CardColour? BackgroundColour = null,
    TextColour? TextColour = null);
