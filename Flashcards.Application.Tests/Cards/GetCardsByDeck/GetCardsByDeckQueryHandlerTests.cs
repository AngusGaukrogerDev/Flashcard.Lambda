using Flashcards.Application.Cards;
using Flashcards.Application.Cards.GetCardsByDeck;
using Flashcards.Application.Decks;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Cards.GetCardsByDeck;

public class GetCardsByDeckQueryHandlerTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "user-456";

    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly GetCardsByDeckQueryHandler _sut;

    public GetCardsByDeckQueryHandlerTests()
    {
        _cardRepository = Substitute.For<ICardRepository>();
        _deckRepository = Substitute.For<IDeckRepository>();
        _sut = new GetCardsByDeckQueryHandler(_cardRepository, _deckRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ReturnsDeckId()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, UserId);
        _cardRepository.GetByDeckIdAsync(deckId.ToString(), Arg.Any<CancellationToken>())
            .Returns(new List<Card>());

        var result = await _sut.HandleAsync(new GetCardsByDeckQuery(deckId.ToString(), UserId));

        result.DeckId.ShouldBe(deckId.ToString());
    }

    [Fact]
    public async Task HandleAsync_WithCards_ReturnsAllCards()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, UserId);
        var cards = new List<Card>
        {
            Card.Reconstitute(CardId.New(), "Hola", "Hello", deckId.ToString(), UserId, DateTime.UtcNow, null),
            Card.Reconstitute(CardId.New(), "Adios", "Goodbye", deckId.ToString(), UserId, DateTime.UtcNow, null)
        };
        _cardRepository.GetByDeckIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(cards);

        var result = await _sut.HandleAsync(new GetCardsByDeckQuery(deckId.ToString(), UserId));

        result.Cards.Count.ShouldBe(2);
    }

    [Fact]
    public async Task HandleAsync_WithCards_MapsFrontAndBackText()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, UserId);
        var cards = new List<Card>
        {
            Card.Reconstitute(CardId.New(), "Hola", "Hello", deckId.ToString(), UserId, DateTime.UtcNow, null)
        };
        _cardRepository.GetByDeckIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(cards);

        var result = await _sut.HandleAsync(new GetCardsByDeckQuery(deckId.ToString(), UserId));

        result.Cards[0].FrontText.ShouldBe("Hola");
        result.Cards[0].BackText.ShouldBe("Hello");
    }

    [Fact]
    public async Task HandleAsync_WithCards_MapsNextReviewDate()
    {
        var deckId = DeckId.New();
        var nextReview = DateTime.UtcNow.AddDays(1);
        SetupDeck(deckId, UserId);
        var cards = new List<Card>
        {
            Card.Reconstitute(CardId.New(), "Hola", "Hello", deckId.ToString(), UserId, DateTime.UtcNow, nextReview)
        };
        _cardRepository.GetByDeckIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(cards);

        var result = await _sut.HandleAsync(new GetCardsByDeckQuery(deckId.ToString(), UserId));

        result.Cards[0].NextReviewDate.ShouldBe(nextReview);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyDeck_ReturnsEmptyList()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, UserId);
        _cardRepository.GetByDeckIdAsync(deckId.ToString(), Arg.Any<CancellationToken>())
            .Returns(new List<Card>());

        var result = await _sut.HandleAsync(new GetCardsByDeckQuery(deckId.ToString(), UserId));

        result.Cards.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenDeckDoesNotExist_ThrowsDeckNotFoundException()
    {
        _deckRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Deck?)null);

        await Should.ThrowAsync<DeckNotFoundException>(() =>
            _sut.HandleAsync(new GetCardsByDeckQuery("non-existent-deck", UserId)));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckDoesNotExist_DoesNotQueryCards()
    {
        _deckRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Deck?)null);

        await Should.ThrowAsync<DeckNotFoundException>(() =>
            _sut.HandleAsync(new GetCardsByDeckQuery("non-existent-deck", UserId)));

        await _cardRepository.DidNotReceive().GetByDeckIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_ThrowsUnauthorisedDeckAccessException()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, OtherUserId);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() =>
            _sut.HandleAsync(new GetCardsByDeckQuery(deckId.ToString(), UserId)));
    }

    [Fact]
    public async Task HandleAsync_WhenDeckBelongsToDifferentUser_DoesNotQueryCards()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, OtherUserId);

        await Should.ThrowAsync<UnauthorisedDeckAccessException>(() =>
            _sut.HandleAsync(new GetCardsByDeckQuery(deckId.ToString(), UserId)));

        await _cardRepository.DidNotReceive().GetByDeckIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void SetupDeck(DeckId deckId, string userId)
    {
        var deck = Deck.Reconstitute(deckId, "Spanish Verbs", null, DateTime.UtcNow, userId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);
    }
}
