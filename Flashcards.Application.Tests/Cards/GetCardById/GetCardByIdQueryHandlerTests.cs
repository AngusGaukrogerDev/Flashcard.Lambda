using Flashcards.Application.Cards;
using Flashcards.Application.Cards.GetCardById;
using Flashcards.Domain.Cards;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Cards.GetCardById;

public class GetCardByIdQueryHandlerTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "user-456";

    private readonly ICardRepository _cardRepository;
    private readonly GetCardByIdQueryHandler _sut;

    public GetCardByIdQueryHandlerTests()
    {
        _cardRepository = Substitute.For<ICardRepository>();
        _sut = new GetCardByIdQueryHandler(_cardRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ReturnsCorrectFrontText()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var query = new GetCardByIdQuery(cardId.ToString(), UserId);

        var result = await _sut.HandleAsync(query);

        result.FrontText.ShouldBe("Hola");
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ReturnsCorrectBackText()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var query = new GetCardByIdQuery(cardId.ToString(), UserId);

        var result = await _sut.HandleAsync(query);

        result.BackText.ShouldBe("Hello");
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ReturnsCorrectId()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var query = new GetCardByIdQuery(cardId.ToString(), UserId);

        var result = await _sut.HandleAsync(query);

        result.Id.ShouldBe(cardId.Value);
    }

    [Fact]
    public async Task HandleAsync_WithNullNextReviewDate_ReturnsNullNextReviewDate()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", UserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var query = new GetCardByIdQuery(cardId.ToString(), UserId);

        var result = await _sut.HandleAsync(query);

        result.NextReviewDate.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNextReviewDate_ReturnsNextReviewDate()
    {
        var nextReview = DateTime.UtcNow.AddDays(1);
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", UserId, DateTime.UtcNow, nextReview);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var query = new GetCardByIdQuery(cardId.ToString(), UserId);

        var result = await _sut.HandleAsync(query);

        result.NextReviewDate.ShouldBe(nextReview);
    }

    [Fact]
    public async Task HandleAsync_WhenCardDoesNotExist_ThrowsCardNotFoundException()
    {
        _cardRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Card?)null);

        var query = new GetCardByIdQuery("non-existent-id", UserId);

        await Should.ThrowAsync<CardNotFoundException>(() => _sut.HandleAsync(query));
    }

    [Fact]
    public async Task HandleAsync_WhenCardBelongsToDifferentUser_ThrowsUnauthorisedCardAccessException()
    {
        var cardId = CardId.New();
        var card = Card.Reconstitute(cardId, "Hola", "Hello", "deck-1", OtherUserId, DateTime.UtcNow, null);
        _cardRepository.GetByIdAsync(cardId.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var query = new GetCardByIdQuery(cardId.ToString(), UserId);

        await Should.ThrowAsync<UnauthorisedCardAccessException>(() => _sut.HandleAsync(query));
    }
}
