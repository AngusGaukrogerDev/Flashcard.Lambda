using Flashcards.Application.Decks;
using Flashcards.Application.Decks.GetDecks;
using Flashcards.Domain.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Decks.GetDecks;

public class GetDecksQueryHandlerTests
{
    private const string UserId = "user-123";

    private readonly IDeckRepository _deckRepository;
    private readonly GetDecksQueryHandler _sut;

    public GetDecksQueryHandlerTests()
    {
        _deckRepository = Substitute.For<IDeckRepository>();
        _sut = new GetDecksQueryHandler(_deckRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ReturnsMappedDecks()
    {
        var deck = Deck.Reconstitute(DeckId.New(), "Spanish Verbs", null, DateTime.UtcNow, UserId);
        _deckRepository
            .GetByUserIdAsync(UserId, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Deck> { deck }, (string?)null));

        var result = await _sut.HandleAsync(new GetDecksQuery(UserId));

        result.Decks.Count.ShouldBe(1);
        result.Decks[0].Name.ShouldBe("Spanish Verbs");
        result.Decks[0].Id.ShouldBe(deck.Id.Value);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ReturnsNullNextPaginationTokenWhenNoMore()
    {
        _deckRepository
            .GetByUserIdAsync(UserId, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Deck>(), (string?)null));

        var result = await _sut.HandleAsync(new GetDecksQuery(UserId));

        result.NextPaginationToken.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsNextToken_PropagatesNextPaginationToken()
    {
        const string nextToken = "next-page-token";
        _deckRepository
            .GetByUserIdAsync(UserId, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Deck>(), (string?)nextToken));

        var result = await _sut.HandleAsync(new GetDecksQuery(UserId));

        result.NextPaginationToken.ShouldBe(nextToken);
    }

    [Fact]
    public async Task HandleAsync_WithPageSize_PassesPageSizeToRepository()
    {
        _deckRepository
            .GetByUserIdAsync(UserId, 10, null, Arg.Any<CancellationToken>())
            .Returns((new List<Deck>(), (string?)null));

        await _sut.HandleAsync(new GetDecksQuery(UserId, PageSize: 10));

        await _deckRepository.Received(1).GetByUserIdAsync(UserId, 10, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithPaginationToken_PassesTokenToRepository()
    {
        const string token = "some-token";
        _deckRepository
            .GetByUserIdAsync(UserId, null, token, Arg.Any<CancellationToken>())
            .Returns((new List<Deck>(), (string?)null));

        await _sut.HandleAsync(new GetDecksQuery(UserId, PaginationToken: token));

        await _deckRepository.Received(1).GetByUserIdAsync(UserId, null, token, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithDecksHavingDescription_MapsDescriptionCorrectly()
    {
        var deck = Deck.Reconstitute(DeckId.New(), "French Nouns", "Common nouns", DateTime.UtcNow, UserId);
        _deckRepository
            .GetByUserIdAsync(UserId, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Deck> { deck }, (string?)null));

        var result = await _sut.HandleAsync(new GetDecksQuery(UserId));

        result.Decks[0].Description.ShouldBe("Common nouns");
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_PropagatesException()
    {
        _deckRepository
            .GetByUserIdAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<(IReadOnlyList<Deck>, string?)>(_ => throw new InvalidOperationException("DynamoDB unavailable"));

        await Should.ThrowAsync<InvalidOperationException>(() => _sut.HandleAsync(new GetDecksQuery(UserId)));
    }
}
