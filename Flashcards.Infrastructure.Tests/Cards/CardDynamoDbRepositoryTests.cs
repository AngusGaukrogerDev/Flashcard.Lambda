using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Flashcards.Domain.Cards;
using Flashcards.Infrastructure.Cards;
using NSubstitute;
using Shouldly;

namespace Flashcards.Infrastructure.Tests.Cards;

public class CardDynamoDbRepositoryTests
{
    private const string TableName = "cards-table";
    private const string DeckIdIndexName = "DeckId-index";
    private const string UserId = "user-123";
    private const string DeckId = "deck-123";

    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly CardDynamoDbRepository _sut;

    public CardDynamoDbRepositoryTests()
    {
        _dynamoDb = Substitute.For<IAmazonDynamoDB>();
        _sut = new CardDynamoDbRepository(_dynamoDb, TableName, DeckIdIndexName);

        _dynamoDb
            .PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        _dynamoDb
            .DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new DeleteItemResponse());
    }

    public class SaveAsyncTests : CardDynamoDbRepositoryTests
    {
        [Fact]
        public async Task SaveAsync_PutsItemToCorrectTable()
        {
            var card = Card.Create("Hola", "Hello", DeckId, UserId);

            await _sut.SaveAsync(card);

            await _dynamoDb.Received(1).PutItemAsync(
                Arg.Is<PutItemRequest>(r => r.TableName == TableName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SaveAsync_StoresCardId()
        {
            var card = Card.Create("Hola", "Hello", DeckId, UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(card);

            captured!.Item["Id"].S.ShouldBe(card.Id.Value.ToString());
        }

        [Fact]
        public async Task SaveAsync_StoresFrontText()
        {
            var card = Card.Create("Hola", "Hello", DeckId, UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(card);

            captured!.Item["FrontText"].S.ShouldBe("Hola");
        }

        [Fact]
        public async Task SaveAsync_StoresBackText()
        {
            var card = Card.Create("Hola", "Hello", DeckId, UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(card);

            captured!.Item["BackText"].S.ShouldBe("Hello");
        }

        [Fact]
        public async Task SaveAsync_StoresDeckId()
        {
            var card = Card.Create("Hola", "Hello", DeckId, UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(card);

            captured!.Item["DeckId"].S.ShouldBe(DeckId);
        }

        [Fact]
        public async Task SaveAsync_StoresUserId()
        {
            var card = Card.Create("Hola", "Hello", DeckId, UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(card);

            captured!.Item["UserId"].S.ShouldBe(UserId);
        }

        [Fact]
        public async Task SaveAsync_StoresCreatedAtAsRoundtripFormat()
        {
            var card = Card.Create("Hola", "Hello", DeckId, UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(card);

            var stored = DateTime.Parse(captured!.Item["CreatedAt"].S, null,
                System.Globalization.DateTimeStyles.RoundtripKind);
            stored.ShouldBe(card.CreatedAt);
        }

        [Fact]
        public async Task SaveAsync_WithNextReviewDate_StoresNextReviewDate()
        {
            var nextReview = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            var card = Card.Reconstitute(
                CardId.New(), "Hola", "Hello", DeckId, UserId, DateTime.UtcNow, nextReview);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(card);

            captured!.Item.ShouldContainKey("NextReviewDate");
            var stored = DateTime.Parse(captured.Item["NextReviewDate"].S, null,
                System.Globalization.DateTimeStyles.RoundtripKind);
            stored.ShouldBe(nextReview);
        }

        [Fact]
        public async Task SaveAsync_WithoutNextReviewDate_DoesNotStoreNextReviewDateAttribute()
        {
            var card = Card.Create("Hola", "Hello", DeckId, UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(card);

            captured!.Item.ShouldNotContainKey("NextReviewDate");
        }
    }

    public class GetByIdAsyncTests : CardDynamoDbRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_QueriesCorrectTable()
        {
            SetupGetItemResponse(BuildCardItem(CardId.New().ToString(), "Hola", "Hello", DeckId, UserId, DateTime.UtcNow));

            await _sut.GetByIdAsync("some-id");

            await _dynamoDb.Received(1).GetItemAsync(
                Arg.Is<GetItemRequest>(r => r.TableName == TableName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetByIdAsync_QueriesWithCorrectKey()
        {
            var cardId = CardId.New().ToString();
            SetupGetItemResponse(BuildCardItem(cardId, "Hola", "Hello", DeckId, UserId, DateTime.UtcNow));
            GetItemRequest? captured = null;
            _dynamoDb.GetItemAsync(Arg.Do<GetItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new GetItemResponse
                {
                    Item = BuildCardItem(cardId, "Hola", "Hello", DeckId, UserId, DateTime.UtcNow)
                });

            await _sut.GetByIdAsync(cardId);

            captured!.Key["Id"].S.ShouldBe(cardId);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemExists_ReturnsMappedCard()
        {
            var cardId = CardId.New().ToString();
            var createdAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            SetupGetItemResponse(BuildCardItem(cardId, "Hola", "Hello", DeckId, UserId, createdAt));

            var result = await _sut.GetByIdAsync(cardId);

            result.ShouldNotBeNull();
            result.Id.Value.ToString().ShouldBe(cardId);
            result.FrontText.ShouldBe("Hola");
            result.BackText.ShouldBe("Hello");
            result.DeckId.ShouldBe(DeckId);
            result.UserId.ShouldBe(UserId);
            result.CreatedAt.ShouldBe(createdAt);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemHasNextReviewDate_ReturnsCardWithNextReviewDate()
        {
            var cardId = CardId.New().ToString();
            var nextReview = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            var item = BuildCardItem(cardId, "Hola", "Hello", DeckId, UserId, DateTime.UtcNow);
            item["NextReviewDate"] = new AttributeValue { S = nextReview.ToString("O") };
            SetupGetItemResponse(item);

            var result = await _sut.GetByIdAsync(cardId);

            result!.NextReviewDate.ShouldBe(nextReview);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemHasNoNextReviewDate_ReturnsCardWithNullNextReviewDate()
        {
            var cardId = CardId.New().ToString();
            SetupGetItemResponse(BuildCardItem(cardId, "Hola", "Hello", DeckId, UserId, DateTime.UtcNow));

            var result = await _sut.GetByIdAsync(cardId);

            result!.NextReviewDate.ShouldBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemDoesNotExist_ReturnsNull()
        {
            _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

            var result = await _sut.GetByIdAsync("non-existent");

            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_PreservesCreatedAtKind()
        {
            var cardId = CardId.New().ToString();
            var createdAt = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            SetupGetItemResponse(BuildCardItem(cardId, "Hola", "Hello", DeckId, UserId, createdAt));

            var result = await _sut.GetByIdAsync(cardId);

            result!.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        }

        private void SetupGetItemResponse(Dictionary<string, AttributeValue> item)
        {
            _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new GetItemResponse { Item = item });
        }
    }

    public class DeleteAsyncTests : CardDynamoDbRepositoryTests
    {
        [Fact]
        public async Task DeleteAsync_DeletesFromCorrectTable()
        {
            await _sut.DeleteAsync("some-id");

            await _dynamoDb.Received(1).DeleteItemAsync(
                Arg.Is<DeleteItemRequest>(r => r.TableName == TableName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeleteAsync_DeletesWithCorrectKey()
        {
            var cardId = CardId.New().ToString();
            DeleteItemRequest? captured = null;
            _dynamoDb.DeleteItemAsync(Arg.Do<DeleteItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new DeleteItemResponse());

            await _sut.DeleteAsync(cardId);

            captured!.Key["Id"].S.ShouldBe(cardId);
        }
    }

    public class GetByDeckIdAsyncTests : CardDynamoDbRepositoryTests
    {
        [Fact]
        public async Task GetByDeckIdAsync_QueriesCorrectTable()
        {
            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            await _sut.GetByDeckIdAsync(DeckId);

            await _dynamoDb.Received(1).QueryAsync(
                Arg.Is<QueryRequest>(r => r.TableName == TableName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetByDeckIdAsync_QueriesCorrectIndex()
        {
            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            await _sut.GetByDeckIdAsync(DeckId);

            await _dynamoDb.Received(1).QueryAsync(
                Arg.Is<QueryRequest>(r => r.IndexName == DeckIdIndexName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetByDeckIdAsync_FiltersOnDeckId()
        {
            QueryRequest? captured = null;
            _dynamoDb.QueryAsync(Arg.Do<QueryRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            await _sut.GetByDeckIdAsync(DeckId);

            captured!.ExpressionAttributeValues[":deckId"].S.ShouldBe(DeckId);
        }

        [Fact]
        public async Task GetByDeckIdAsync_ReturnsAllCards()
        {
            var cardId1 = CardId.New().ToString();
            var cardId2 = CardId.New().ToString();
            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse
                {
                    Items = new List<Dictionary<string, AttributeValue>>
                    {
                        BuildCardItem(cardId1, "Hola", "Hello", DeckId, UserId, DateTime.UtcNow),
                        BuildCardItem(cardId2, "Adios", "Goodbye", DeckId, UserId, DateTime.UtcNow)
                    }
                });

            var result = await _sut.GetByDeckIdAsync(DeckId);

            result.Count.ShouldBe(2);
        }

        [Fact]
        public async Task GetByDeckIdAsync_ReturnsMappedCards()
        {
            var cardId = CardId.New().ToString();
            var createdAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse
                {
                    Items = new List<Dictionary<string, AttributeValue>>
                    {
                        BuildCardItem(cardId, "Hola", "Hello", DeckId, UserId, createdAt)
                    }
                });

            var result = await _sut.GetByDeckIdAsync(DeckId);

            result[0].Id.Value.ToString().ShouldBe(cardId);
            result[0].FrontText.ShouldBe("Hola");
            result[0].BackText.ShouldBe("Hello");
            result[0].DeckId.ShouldBe(DeckId);
            result[0].CreatedAt.ShouldBe(createdAt);
        }

        [Fact]
        public async Task GetByDeckIdAsync_WithEmptyDeck_ReturnsEmptyList()
        {
            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            var result = await _sut.GetByDeckIdAsync(DeckId);

            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetByDeckIdAsync_WithoutIndexConfigured_ThrowsInvalidOperationException()
        {
            var repoWithoutIndex = new CardDynamoDbRepository(_dynamoDb, TableName);

            await Should.ThrowAsync<InvalidOperationException>(() =>
                repoWithoutIndex.GetByDeckIdAsync(DeckId));
        }
    }

    private static Dictionary<string, AttributeValue> BuildCardItem(
        string id, string frontText, string backText, string deckId, string userId, DateTime createdAt)
        => new()
        {
            ["Id"] = new() { S = id },
            ["FrontText"] = new() { S = frontText },
            ["BackText"] = new() { S = backText },
            ["DeckId"] = new() { S = deckId },
            ["UserId"] = new() { S = userId },
            ["CreatedAt"] = new() { S = createdAt.ToString("O") }
        };
}
