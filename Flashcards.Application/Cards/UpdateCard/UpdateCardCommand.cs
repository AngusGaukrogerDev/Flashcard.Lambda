namespace Flashcards.Application.Cards.UpdateCard;

public record UpdateCardCommand(string CardId, string UserId, string FrontText, string BackText);
