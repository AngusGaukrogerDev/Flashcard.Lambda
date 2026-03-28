using Flashcards.Application.Cards;
using Flashcards.Application.Cards.UpdateCard;
using Flashcards.Application.DeckTags;
using Flashcards.Domain.Cards;
using Flashcards.Domain.DeckTags;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Cards.UpdateCard;

public class UpdateCardCommandHandlerTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "user-456";

    private readonly ICardRepository _cardRepository;
    private readonly IDeckTagReadRepository _deckTagReadRepository;
    private readonly UpdateCardCommandHandler _sut;

    public UpdateCardCommandHandlerTests()
    {
        _cardRepository = Substitute.For<ICardRepository>();
        _deckTagReadRepository = Substitute.For<IDeckTagReadRepository>();
        _deckTagReadRepository.GetByDeckIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<DeckTag>());
        _sut = new UpdateCardCommandHandler(_cardRepository, _deckTagReadRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsUpdatedFrontText()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, "New Front", "New Back");

        var result = await _sut.HandleAsync(command);

        result.FrontText.ShouldBe("New Front");
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsUpdatedBackText()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, "New Front", "New Back");

        var result = await _sut.HandleAsync(command);

        result.BackText.ShouldBe("New Back");
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_RetainsOriginalId()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, "New Front", "New Back");

        var result = await _sut.HandleAsync(command);

        result.Id.ShouldBe(cardId.Value);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesUpdatedCard()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, "New Front", "New Back");

        await _sut.HandleAsync(command);

        await _cardRepository.Received(1).SaveAsync(
            Arg.Is<Card>(c => c.FrontText == "New Front" && c.BackText == "New Back"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCardDoesNotExist_ThrowsCardNotFoundException()
    {
        _cardRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Card?)null);

        var command = new UpdateCardCommand("non-existent-id", UserId, "New Front", "New Back");

        await Should.ThrowAsync<CardNotFoundException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenCardBelongsToDifferentUser_ThrowsUnauthorisedCardAccessException()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", OtherUserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, "New Front", "New Back");

        await Should.ThrowAsync<UnauthorisedCardAccessException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenCardBelongsToDifferentUser_DoesNotSave()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", OtherUserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, "New Front", "New Back");

        await Should.ThrowAsync<UnauthorisedCardAccessException>(() => _sut.HandleAsync(command));

        await _cardRepository.DidNotReceive().SaveAsync(Arg.Any<Card>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithBlankFrontText_ThrowsArgumentException(string blankText)
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, blankText, "New Back");

        await Should.ThrowAsync<ArgumentException>(() => _sut.HandleAsync(command));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithBlankBackText_ThrowsArgumentException(string blankText)
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, "New Front", blankText);

        await Should.ThrowAsync<ArgumentException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithTextSurroundedByWhitespace_TrimsText()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Old Front", "Old Back", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var command = new UpdateCardCommand(cardId.ToString(), UserId, "  New Front  ", "  New Back  ");

        var result = await _sut.HandleAsync(command);

        result.FrontText.ShouldBe("New Front");
        result.BackText.ShouldBe("New Back");
    }
}
