using Flashcards.Application.Cards;
using Flashcards.Application.Decks;
using Flashcards.Application.Decks.GetDeckStats;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Decks.GetDeckStats;

public class GetDeckStatsQueryHandlerTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "user-456";

    private readonly IDeckRepository _deckRepository;
    private readonly ICardRepository _cardRepository;
    private readonly GetDeckStatsQueryHandler _sut;

    public GetDeckStatsQueryHandlerTests()
    {
        _deckRepository = Substitute.For<IDeckRepository>();
        _cardRepository = Substitute.For<ICardRepository>();
        _sut = new GetDeckStatsQueryHandler(_deckRepository, _cardRepository);
    }

    [Fact]
    public async Task HandleAsync_WithMixedRatings_ReturnsCounts()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, UserId);

        var cards = new List<Card>
        {
            Card.Reconstitute(CardId.New(), "F1", "B1", deckId.ToString(), UserId, DateTime.UtcNow, null, lastRecallRating: null),
            Card.Reconstitute(CardId.New(), "F2", "B2", deckId.ToString(), UserId, DateTime.UtcNow, null, lastRecallRating: RecallRating.Incorrect),
            Card.Reconstitute(CardId.New(), "F3", "B3", deckId.ToString(), UserId, DateTime.UtcNow, null, lastRecallRating: RecallRating.Hard),
            Card.Reconstitute(CardId.New(), "F4", "B4", deckId.ToString(), UserId, DateTime.UtcNow, null, lastRecallRating: RecallRating.Medium),
            Card.Reconstitute(CardId.New(), "F5", "B5", deckId.ToString(), UserId, DateTime.UtcNow, null, lastRecallRating: RecallRating.Easy),
            Card.Reconstitute(CardId.New(), "F6", "B6", deckId.ToString(), UserId, DateTime.UtcNow, null, lastRecallRating: RecallRating.Easy),
        };

        _cardRepository.GetAllByDeckIdAsync(deckId.ToString(), Arg.Any<CancellationToken>())
            .Returns(cards);

        var result = await _sut.HandleAsync(new GetDeckStatsQuery(deckId.ToString(), UserId));

        result.DeckId.ShouldBe(deckId.ToString());
        result.TotalCards.ShouldBe(6);
        result.NewCount.ShouldBe(1);
        result.IncorrectCount.ShouldBe(1);
        result.HardCount.ShouldBe(1);
        result.MediumCount.ShouldBe(1);
        result.EasyCount.ShouldBe(2);
    }

    [Fact]
    public async Task HandleAsync_WhenDeckDoesNotExist_ThrowsDeckNotFoundException()
    {
        _deckRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Deck?)null);

        await Should.ThrowAsync<DeckNotFoundException>(() =>
            _sut.HandleAsync(new GetDeckStatsQuery("non-existent-deck", UserId)));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_ThrowsUnauthorisedDeckAccessException()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, OtherUserId);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() =>
            _sut.HandleAsync(new GetDeckStatsQuery(deckId.ToString(), UserId)));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_DoesNotQueryCards()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, OtherUserId);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() =>
            _sut.HandleAsync(new GetDeckStatsQuery(deckId.ToString(), UserId)));

        await _cardRepository.DidNotReceive().GetAllByDeckIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void SetupDeck(DeckId deckId, string userId)
    {
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, userId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);
    }
}

