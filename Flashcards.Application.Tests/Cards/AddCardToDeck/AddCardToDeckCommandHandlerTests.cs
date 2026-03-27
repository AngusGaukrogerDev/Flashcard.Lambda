using Flashcards.Application.Cards;
using Flashcards.Application.Cards.AddCardToDeck;
using Flashcards.Application.Decks;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Cards.AddCardToDeck;

public class AddCardToDeckCommandHandlerTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "user-456";

    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly AddCardToDeckCommandHandler _sut;

    public AddCardToDeckCommandHandlerTests()
    {
        _cardRepository = Substitute.For<ICardRepository>();
        _deckRepository = Substitute.For<IDeckRepository>();
        _sut = new AddCardToDeckCommandHandler(_cardRepository, _deckRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsResponseWithCorrectFrontText()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        var result = await _sut.HandleAsync(command);

        result.FrontText.ShouldBe("Hola");
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsResponseWithCorrectBackText()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        var result = await _sut.HandleAsync(command);

        result.BackText.ShouldBe("Hello");
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsResponseWithCorrectDeckId()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        var result = await _sut.HandleAsync(command);

        result.DeckId.ShouldBe(deckId.ToString());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsNonEmptyId()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        var result = await _sut.HandleAsync(command);

        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsRecentCreatedAt()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var before = DateTime.UtcNow;
        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        var result = await _sut.HandleAsync(command);

        result.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        result.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesCardToRepository()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        await _sut.HandleAsync(command);

        await _cardRepository.Received(1).SaveAsync(
            Arg.Is<Card>(c => c.FrontText == "Hola" && c.BackText == "Hello"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesCardWithUserId()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        await _sut.HandleAsync(command);

        await _cardRepository.Received(1).SaveAsync(
            Arg.Is<Card>(c => c.UserId.Value == UserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_EachInvocation_ProducesUniqueCardId()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        var first = await _sut.HandleAsync(command);
        var second = await _sut.HandleAsync(command);

        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenDeckDoesNotExist_ThrowsDeckNotFoundException()
    {
        _deckRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Deck?)null);

        var command = new AddCardToDeckCommand("Hola", "Hello", "non-existent-deck-id", UserId);

        await Should.ThrowAsync<DeckNotFoundException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckDoesNotExist_DoesNotSaveCard()
    {
        _deckRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Deck?)null);

        var command = new AddCardToDeckCommand("Hola", "Hello", "non-existent-deck-id", UserId);

        await Should.ThrowAsync<DeckNotFoundException>(() => _sut.HandleAsync(command));

        await _cardRepository.DidNotReceive().SaveAsync(Arg.Any<Card>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_ThrowsUnauthorisedDeckAccessException()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, OtherUserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_DoesNotSaveCard()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, OtherUserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", "Hello", deckId.ToString(), UserId);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() => _sut.HandleAsync(command));

        await _cardRepository.DidNotReceive().SaveAsync(Arg.Any<Card>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithBlankFrontText_ThrowsArgumentException(string blankText)
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand(blankText, "Hello", deckId.ToString(), UserId);

        await Should.ThrowAsync<ArgumentException>(() => _sut.HandleAsync(command));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithBlankBackText_ThrowsArgumentException(string blankText)
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("Hola", blankText, deckId.ToString(), UserId);

        await Should.ThrowAsync<ArgumentException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithTextSurroundedByWhitespace_TrimsText()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new AddCardToDeckCommand("  Hola  ", "  Hello  ", deckId.ToString(), UserId);

        var result = await _sut.HandleAsync(command);

        result.FrontText.ShouldBe("Hola");
        result.BackText.ShouldBe("Hello");
    }
}
