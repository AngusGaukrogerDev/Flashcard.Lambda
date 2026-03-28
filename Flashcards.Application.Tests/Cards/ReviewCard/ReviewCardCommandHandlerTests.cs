using Flashcards.Application.Cards;
using Flashcards.Application.Cards.ReviewCard;
using Flashcards.Domain.Cards;
using NSubstitute;
using Shouldly;

namespace Flashcards.Application.Tests.Cards.ReviewCard;

public class ReviewCardCommandHandlerTests
{
    private const string UserId = "user-123";

    private readonly ICardRepository _cardRepository;
    private readonly ReviewCardCommandHandler _sut;

    public ReviewCardCommandHandlerTests()
    {
        _cardRepository = Substitute.For<ICardRepository>();
        _sut = new ReviewCardCommandHandler(_cardRepository);
    }

    [Fact]
    public async Task HandleAsync_AppliesRatingAndPersists()
    {
        var card = Card.Create("a", "b", "deck-1", UserId);
        _cardRepository.GetByIdAsync(card.Id.Value.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        var result = await _sut.HandleAsync(new ReviewCardCommand(card.Id.Value.ToString(), UserId, RecallRating.Medium));

        await _cardRepository.Received(1).SaveAsync(card, Arg.Any<CancellationToken>());
        result.LastRecallRating.ShouldBe(RecallRating.Medium);
        result.RecallTrafficLight.ShouldBe(RecallTrafficLight.Yellow);
        result.NextReviewDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenWrongUser_ThrowsUnauthorisedCardAccessException()
    {
        var card = Card.Create("a", "b", "deck-1", "other-user");
        _cardRepository.GetByIdAsync(card.Id.Value.ToString(), Arg.Any<CancellationToken>()).Returns(card);

        await Should.ThrowAsync<UnauthorisedCardAccessException>(() =>
            _sut.HandleAsync(new ReviewCardCommand(card.Id.Value.ToString(), UserId, RecallRating.Easy)));
    }
}
