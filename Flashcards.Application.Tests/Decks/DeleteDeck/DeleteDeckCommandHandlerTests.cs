using Flashcards.Application.Decks;
using Flashcards.Application.Decks.DeleteDeck;
using Flashcards.Domain.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Decks.DeleteDeck;

public class DeleteDeckCommandHandlerTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "user-456";

    private readonly IDeckRepository _deckRepository;
    private readonly DeleteDeckCommandHandler _sut;

    public DeleteDeckCommandHandlerTests()
    {
        _deckRepository = Substitute.For<IDeckRepository>();
        _sut = new DeleteDeckCommandHandler(_deckRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_DeletesDeckFromRepository()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new DeleteDeckCommand(deckId.ToString(), UserId);
        await _sut.HandleAsync(command);

        await _deckRepository.Received(1).DeleteAsync(deckId.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeckDoesNotExist_ThrowsDeckNotFoundException()
    {
        _deckRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Deck?)null);

        var command = new DeleteDeckCommand("non-existent-id", UserId);

        await Should.ThrowAsync<DeckNotFoundException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_ThrowsUnauthorisedDeckAccessException()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, OtherUserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new DeleteDeckCommand(deckId.ToString(), UserId);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_DoesNotDelete()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, OtherUserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);

        var command = new DeleteDeckCommand(deckId.ToString(), UserId);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() => _sut.HandleAsync(command));

        await _deckRepository.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsOnDelete_PropagatesException()
    {
        var deckId = DeckId.New();
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);
        _deckRepository
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("DynamoDB unavailable"));

        var command = new DeleteDeckCommand(deckId.ToString(), UserId);

        await Should.ThrowAsync<InvalidOperationException>(() => _sut.HandleAsync(command));
    }
}
