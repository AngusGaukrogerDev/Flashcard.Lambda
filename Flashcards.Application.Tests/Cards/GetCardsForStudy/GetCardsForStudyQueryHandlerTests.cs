using Flashcards.Application.Cards;
using Flashcards.Application.Cards.GetCardsForStudy;
using Flashcards.Application.Decks;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Cards.GetCardsForStudy;

public class GetCardsForStudyQueryHandlerTests
{
    private const string UserId = "user-123";

    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly GetCardsForStudyQueryHandler _sut;

    public GetCardsForStudyQueryHandlerTests()
    {
        _cardRepository = Substitute.For<ICardRepository>();
        _deckRepository = Substitute.For<IDeckRepository>();
        _sut = new GetCardsForStudyQueryHandler(_cardRepository, _deckRepository);
    }

    [Fact]
    public async Task HandleAsync_OrdersDueFirst()
    {
        var deckId = DeckId.New();
        SetupDeck(deckId, UserId);
        var now = DateTime.UtcNow;
        var due = Card.Reconstitute(CardId.New(), "a", "b", deckId.ToString(), UserId, DateTime.UtcNow, now.AddDays(-1));
        var upcoming = Card.Reconstitute(CardId.New(), "c", "d", deckId.ToString(), UserId, DateTime.UtcNow, now.AddDays(3));
        _cardRepository.GetAllByDeckIdAsync(deckId.ToString(), Arg.Any<CancellationToken>())
            .Returns(new[] { upcoming, due });

        var result = await _sut.HandleAsync(new GetCardsForStudyQuery(deckId.ToString(), UserId, 10));

        result.Cards[0].FrontText.ShouldBe("a");
        result.Cards[0].IsDue.ShouldBeTrue();
        result.Cards[1].IsDue.ShouldBeFalse();
        result.DueCount.ShouldBe(1);
        result.UpcomingCount.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_WhenDeckMissing_ThrowsDeckNotFoundException()
    {
        _deckRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Deck?)null);

        await Should.ThrowAsync<DeckNotFoundException>(() =>
            _sut.HandleAsync(new GetCardsForStudyQuery("missing", UserId, 10)));
    }

    private void SetupDeck(DeckId deckId, string userId)
    {
        var deck = Deck.Reconstitute(deckId, "Deck", null, DateTime.UtcNow, userId);
        _deckRepository.GetByIdAsync(deckId.ToString(), Arg.Any<CancellationToken>()).Returns(deck);
    }
}
