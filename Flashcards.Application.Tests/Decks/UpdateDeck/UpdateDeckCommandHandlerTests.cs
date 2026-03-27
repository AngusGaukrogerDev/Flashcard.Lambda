using Flashcards.Application.Decks;
using Flashcards.Application.Decks.UpdateDeck;
using Flashcards.Domain.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Decks.UpdateDeck;

public class UpdateDeckCommandHandlerTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "user-456";

    private readonly IDeckRepository _deckRepository;
    private readonly UpdateDeckCommandHandler _sut;

    public UpdateDeckCommandHandlerTests()
    {
        _deckRepository = Substitute.For<IDeckRepository>();
        _sut = new UpdateDeckCommandHandler(_deckRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsUpdatedName()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Old Name", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new UpdateDeckCommand(deckId.ToString(), UserId, "New Name", null);
        var result = await _sut.HandleAsync(command);

        result.Name.ShouldBe("New Name");
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsUpdatedDescription()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", "Old description", DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new UpdateDeckCommand(deckId.ToString(), UserId, "Spanish Verbs", "New description");
        var result = await _sut.HandleAsync(command);

        result.Description.ShouldBe("New description");
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesUpdatedDeck()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Old Name", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new UpdateDeckCommand(deckId.ToString(), UserId, "New Name", null);
        await _sut.HandleAsync(command);

        await _deckRepository.Received(1).SaveAsync(
            Arg.Is<Deck>(d => d.Name == "New Name"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_RetainsOriginalId()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Old Name", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new UpdateDeckCommand(deckId.ToString(), UserId, "New Name", null);
        var result = await _sut.HandleAsync(command);

        result.Id.ShouldBe(deckId.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenDeckDoesNotExist_ThrowsDeckNotFoundException()
    {
        _deckRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Deck?)null);

        var command = new UpdateDeckCommand("non-existent-id", UserId, "New Name", null);

        await Should.ThrowAsync<DeckNotFoundException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_ThrowsUnauthorisedDeckAccessException()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, OtherUserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new UpdateDeckCommand(deckId.ToString(), UserId, "New Name", null);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_DoesNotSave()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, OtherUserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new UpdateDeckCommand(deckId.ToString(), UserId, "New Name", null);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() => _sut.HandleAsync(command));

        await _deckRepository.DidNotReceive().SaveAsync(Arg.Any<Deck>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithBlankName_ThrowsArgumentException(string blankName)
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new UpdateDeckCommand(deckId.ToString(), UserId, blankName, null);

        await Should.ThrowAsync<ArgumentException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithNameSurroundedByWhitespace_TrimsName()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Old Name", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new UpdateDeckCommand(deckId.ToString(), UserId, "  New Name  ", null);
        var result = await _sut.HandleAsync(command);

        result.Name.ShouldBe("New Name");
    }
}
