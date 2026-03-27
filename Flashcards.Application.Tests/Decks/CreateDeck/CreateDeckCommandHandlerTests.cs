using Flashcards.Application.Decks;
using Flashcards.Application.Decks.CreateDeck;
using Flashcards.Domain.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Decks.CreateDeck;

public class CreateDeckCommandHandlerTests
{
    private readonly IDeckRepository _deckRepository;
    private readonly CreateDeckCommandHandler _sut;

    public CreateDeckCommandHandlerTests()
    {
        _deckRepository = Substitute.For<IDeckRepository>();
        _sut = new CreateDeckCommandHandler(_deckRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsDeckWithCorrectName()
    {
        var command = new CreateDeckCommand("Spanish Verbs", null);

        var result = await _sut.HandleAsync(command);

        result.Name.ShouldBe("Spanish Verbs");
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsNonEmptyId()
    {
        var command = new CreateDeckCommand("Spanish Verbs", null);

        var result = await _sut.HandleAsync(command);

        result.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsRecentCreatedAt()
    {
        var before = DateTime.UtcNow;
        var command = new CreateDeckCommand("Spanish Verbs", null);

        var result = await _sut.HandleAsync(command);

        result.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        result.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_SavesDeckToRepository()
    {
        var command = new CreateDeckCommand("Spanish Verbs", null);

        await _sut.HandleAsync(command);

        await _deckRepository.Received(1).SaveAsync(
            Arg.Is<Deck>(d => d.Name == "Spanish Verbs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithDescription_SavesDeckWithDescription()
    {
        var command = new CreateDeckCommand("French Nouns", "Common French nouns for everyday use");

        await _sut.HandleAsync(command);

        await _deckRepository.Received(1).SaveAsync(
            Arg.Is<Deck>(d => d.Description == "Common French nouns for everyday use"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_EachInvocation_ProducesUniqueDeckId()
    {
        var command = new CreateDeckCommand("Spanish Verbs", null);

        var first = await _sut.HandleAsync(command);
        var second = await _sut.HandleAsync(command);

        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public async Task HandleAsync_WithNameSurroundedByWhitespace_TrimsName()
    {
        var command = new CreateDeckCommand("  Spanish Verbs  ", null);

        var result = await _sut.HandleAsync(command);

        result.Name.ShouldBe("Spanish Verbs");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithBlankName_ThrowsArgumentException(string blankName)
    {
        var command = new CreateDeckCommand(blankName, null);

        await Should.ThrowAsync<ArgumentException>(() => _sut.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_PropagatesException()
    {
        _deckRepository
            .SaveAsync(Arg.Any<Deck>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("DynamoDB unavailable"));

        var command = new CreateDeckCommand("Spanish Verbs", null);

        await Should.ThrowAsync<InvalidOperationException>(() => _sut.HandleAsync(command));
    }
}
