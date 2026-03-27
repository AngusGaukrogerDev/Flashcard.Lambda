using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Flashcards.Domain.Decks;
using Flashcards.Infrastructure.Decks;
using NSubstitute;
using Shouldly;

namespace Flashcards.Infrastructure.Tests.Decks;

public class DeckDynamoDbRepositoryTests
{
    private const string TableName = "decks-table";
    private const string UserIdIndexName = "userId-index";
    private const string UserId = "user-123";

    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DeckDynamoDbRepository _sut;

    public DeckDynamoDbRepositoryTests()
    {
        _dynamoDb = Substitute.For<IAmazonDynamoDB>();
        _sut = new DeckDynamoDbRepository(_dynamoDb, TableName, UserIdIndexName);

        _dynamoDb
            .PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        _dynamoDb
            .DeleteItemAsync(Arg.Any<DeleteItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new DeleteItemResponse());
    }

    public class SaveAsyncTests : DeckDynamoDbRepositoryTests
    {
        [Fact]
        public async Task SaveAsync_PutsItemToCorrectTable()
        {
            var deck = Deck.Create("Spanish Verbs", UserId);

            await _sut.SaveAsync(deck);

            await _dynamoDb.Received(1).PutItemAsync(
                Arg.Is<PutItemRequest>(r => r.TableName == TableName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SaveAsync_StoresDeckId()
        {
            var deck = Deck.Create("Spanish Verbs", UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(deck);

            captured!.Item["Id"].S.ShouldBe(deck.Id.Value.ToString());
        }

        [Fact]
        public async Task SaveAsync_StoresDeckName()
        {
            var deck = Deck.Create("Spanish Verbs", UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(deck);

            captured!.Item["Name"].S.ShouldBe("Spanish Verbs");
        }

        [Fact]
        public async Task SaveAsync_StoresUserId()
        {
            var deck = Deck.Create("Spanish Verbs", UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(deck);

            captured!.Item["UserId"].S.ShouldBe(UserId);
        }

        [Fact]
        public async Task SaveAsync_StoresCreatedAtAsRoundtripFormat()
        {
            var deck = Deck.Create("Spanish Verbs", UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(deck);

            var storedDate = DateTime.Parse(captured!.Item["CreatedAt"].S, null,
                System.Globalization.DateTimeStyles.RoundtripKind);
            storedDate.ShouldBe(deck.CreatedAt);
        }

        [Fact]
        public async Task SaveAsync_WithDescription_StoresDescription()
        {
            var deck = Deck.Create("Spanish Verbs", UserId, "Common verbs");
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(deck);

            captured!.Item["Description"].S.ShouldBe("Common verbs");
        }

        [Fact]
        public async Task SaveAsync_WithoutDescription_DoesNotStoreDescriptionAttribute()
        {
            var deck = Deck.Create("Spanish Verbs", UserId);
            PutItemRequest? captured = null;
            _dynamoDb.PutItemAsync(Arg.Do<PutItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new PutItemResponse());

            await _sut.SaveAsync(deck);

            captured!.Item.ShouldNotContainKey("Description");
        }
    }

    public class GetByIdAsyncTests : DeckDynamoDbRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_QueriesCorrectTable()
        {
            SetupGetItemResponse(DeckId.New().ToString(), "Spanish Verbs", UserId, DateTime.UtcNow);

            await _sut.GetByIdAsync("some-id");

            await _dynamoDb.Received(1).GetItemAsync(
                Arg.Is<GetItemRequest>(r => r.TableName == TableName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetByIdAsync_QueriesWithCorrectKey()
        {
            var deckId = DeckId.New().ToString();
            SetupGetItemResponse(deckId, "Spanish Verbs", UserId, DateTime.UtcNow);
            GetItemRequest? captured = null;
            _dynamoDb.GetItemAsync(Arg.Do<GetItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new GetItemResponse { Item = BuildDeckItem(deckId, "Spanish Verbs", UserId, DateTime.UtcNow) });

            await _sut.GetByIdAsync(deckId);

            captured!.Key["Id"].S.ShouldBe(deckId);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemExists_ReturnsCorrectDeck()
        {
            var deckId = DeckId.New().ToString();
            var createdAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            SetupGetItemResponse(deckId, "Spanish Verbs", UserId, createdAt);

            var result = await _sut.GetByIdAsync(deckId);

            result.ShouldNotBeNull();
            result.Id.Value.ToString().ShouldBe(deckId);
            result.Name.ShouldBe("Spanish Verbs");
            result.UserId.Value.ShouldBe(UserId);
            result.CreatedAt.ShouldBe(createdAt);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemHasDescription_ReturnsDeckWithDescription()
        {
            var deckId = DeckId.New().ToString();
            var item = BuildDeckItem(deckId, "Spanish Verbs", UserId, DateTime.UtcNow);
            item["Description"] = new AttributeValue { S = "Common verbs" };
            _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new GetItemResponse { Item = item });

            var result = await _sut.GetByIdAsync(deckId);

            result!.Description.ShouldBe("Common verbs");
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemHasNoDescription_ReturnsDeckWithNullDescription()
        {
            var deckId = DeckId.New().ToString();
            SetupGetItemResponse(deckId, "Spanish Verbs", UserId, DateTime.UtcNow);

            var result = await _sut.GetByIdAsync(deckId);

            result!.Description.ShouldBeNull();
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
            var deckId = DeckId.New().ToString();
            var createdAt = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            SetupGetItemResponse(deckId, "Spanish Verbs", UserId, createdAt);

            var result = await _sut.GetByIdAsync(deckId);

            result!.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        }

        private void SetupGetItemResponse(string deckId, string name, string userId, DateTime createdAt)
        {
            _dynamoDb.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new GetItemResponse { Item = BuildDeckItem(deckId, name, userId, createdAt) });
        }
    }

    public class GetByUserIdAsyncTests : DeckDynamoDbRepositoryTests
    {
        [Fact]
        public async Task GetByUserIdAsync_QueriesCorrectTable()
        {
            SetupQueryResponse(new List<Dictionary<string, AttributeValue>>());

            await _sut.GetByUserIdAsync(UserId);

            await _dynamoDb.Received(1).QueryAsync(
                Arg.Is<QueryRequest>(r => r.TableName == TableName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetByUserIdAsync_QueriesCorrectIndex()
        {
            SetupQueryResponse(new List<Dictionary<string, AttributeValue>>());

            await _sut.GetByUserIdAsync(UserId);

            await _dynamoDb.Received(1).QueryAsync(
                Arg.Is<QueryRequest>(r => r.IndexName == UserIdIndexName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetByUserIdAsync_FiltersOnUserId()
        {
            QueryRequest? captured = null;
            _dynamoDb.QueryAsync(Arg.Do<QueryRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            await _sut.GetByUserIdAsync(UserId);

            captured!.ExpressionAttributeValues[":userId"].S.ShouldBe(UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsDecks()
        {
            var deckId = DeckId.New().ToString();
            SetupQueryResponse(new List<Dictionary<string, AttributeValue>>
            {
                BuildDeckItem(deckId, "Spanish Verbs", UserId, DateTime.UtcNow)
            });

            var (decks, _) = await _sut.GetByUserIdAsync(UserId);

            decks.Count.ShouldBe(1);
            decks[0].Name.ShouldBe("Spanish Verbs");
        }

        [Fact]
        public async Task GetByUserIdAsync_WithPageSize_SetsLimit()
        {
            QueryRequest? captured = null;
            _dynamoDb.QueryAsync(Arg.Do<QueryRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            await _sut.GetByUserIdAsync(UserId, pageSize: 10);

            captured!.Limit.ShouldBe(10);
        }

        [Fact]
        public async Task GetByUserIdAsync_WithoutPageSize_DoesNotSetLimit()
        {
            QueryRequest? captured = null;
            _dynamoDb.QueryAsync(Arg.Do<QueryRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            await _sut.GetByUserIdAsync(UserId);

            captured!.Limit.ShouldBeNull();
        }

        [Fact]
        public async Task GetByUserIdAsync_WhenLastEvaluatedKeyExists_ReturnsEncodedPaginationToken()
        {
            var lastKey = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new() { S = "some-deck-id" },
                ["UserId"] = new() { S = UserId }
            };
            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new(), LastEvaluatedKey = lastKey });

            var (_, nextToken) = await _sut.GetByUserIdAsync(UserId);

            nextToken.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetByUserIdAsync_WhenNoLastEvaluatedKey_ReturnsNullPaginationToken()
        {
            SetupQueryResponse(new List<Dictionary<string, AttributeValue>>());

            var (_, nextToken) = await _sut.GetByUserIdAsync(UserId);

            nextToken.ShouldBeNull();
        }

        [Fact]
        public async Task GetByUserIdAsync_WithPaginationToken_SetsExclusiveStartKey()
        {
            var lastKey = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new() { S = "some-deck-id" },
                ["UserId"] = new() { S = UserId }
            };

            QueryRequest? captured = null;
            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new(), LastEvaluatedKey = lastKey });

            var (_, token) = await _sut.GetByUserIdAsync(UserId);

            _dynamoDb.QueryAsync(Arg.Do<QueryRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            await _sut.GetByUserIdAsync(UserId, paginationToken: token!);

            captured!.ExclusiveStartKey["Id"].S.ShouldBe("some-deck-id");
            captured!.ExclusiveStartKey["UserId"].S.ShouldBe(UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_PaginationToken_RoundTripsCorrectly()
        {
            var lastKey = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new() { S = "round-trip-id" },
                ["UserId"] = new() { S = UserId }
            };

            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new(), LastEvaluatedKey = lastKey },
                         new QueryResponse { Items = new() });

            var (_, nextToken) = await _sut.GetByUserIdAsync(UserId);

            QueryRequest? secondRequest = null;
            _dynamoDb.QueryAsync(Arg.Do<QueryRequest>(r => secondRequest = r), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = new() });

            await _sut.GetByUserIdAsync(UserId, paginationToken: nextToken);

            secondRequest!.ExclusiveStartKey["Id"].S.ShouldBe("round-trip-id");
            secondRequest!.ExclusiveStartKey["UserId"].S.ShouldBe(UserId);
        }

        private void SetupQueryResponse(List<Dictionary<string, AttributeValue>> items)
        {
            _dynamoDb.QueryAsync(Arg.Any<QueryRequest>(), Arg.Any<CancellationToken>())
                .Returns(new QueryResponse { Items = items, LastEvaluatedKey = new() });
        }
    }

    public class DeleteAsyncTests : DeckDynamoDbRepositoryTests
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
            var deckId = DeckId.New().ToString();
            DeleteItemRequest? captured = null;
            _dynamoDb.DeleteItemAsync(Arg.Do<DeleteItemRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new DeleteItemResponse());

            await _sut.DeleteAsync(deckId);

            captured!.Key["Id"].S.ShouldBe(deckId);
        }
    }

    private static Dictionary<string, AttributeValue> BuildDeckItem(
        string id, string name, string userId, DateTime createdAt)
        => new()
        {
            ["Id"] = new() { S = id },
            ["Name"] = new() { S = name },
            ["UserId"] = new() { S = userId },
            ["CreatedAt"] = new() { S = createdAt.ToString("O") }
        };
}
