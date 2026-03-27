using Flashcards.Application.Cards;
using Flashcards.Application.Cards.DeleteCard;
using Flashcards.Domain.Cards;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Cards.DeleteCard;

public class DeleteCardCommandHandlerTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "user-456";

    private readonly ICardRepository _cardRepository;
    private readonly DeleteCardCommandHandler _sut;

    public DeleteCardCommandHandlerTests()
    {
        _cardRepository = Substitute.For<ICardRepository>();
        _sut = new DeleteCardCommandHandler(_cardRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_DeletesCard()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new DeleteCardCommand(cardId.ToString(), UserId);

        await _sut.HandleAsync(command);

        await _cardRepository.Received(1).DeleteAsync(cardId.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCardDoesNotExist_ThrowsCardNotFoundException()
    {
        _cardRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Card?)null);

        var command = new DeleteCardCommand("non-existent-id", UserId);

        await Should.ThrowAsync<CardNotFoundException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenCardDoesNotExist_DoesNotDelete()
    {
        _cardRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Card?)null);

        var command = new DeleteCardCommand("non-existent-id", UserId);

        await Should.ThrowAsync<CardNotFoundException>(() => _sut.HandleAsync(command));

        await _cardRepository.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCardBelongsToDifferentUser_ThrowsUnauthorisedCardAccessException()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", OtherUserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new DeleteCardCommand(cardId.ToString(), UserId);

        await Should.ThrowAsync<UnauthorisedCardAccessException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenCardBelongsToDifferentUser_DoesNotDelete()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", OtherUserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new DeleteCardCommand(cardId.ToString(), UserId);

        await Should.ThrowAsync<UnauthorisedCardAccessException>(() => _sut.HandleAsync(command));

        await _cardRepository.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
