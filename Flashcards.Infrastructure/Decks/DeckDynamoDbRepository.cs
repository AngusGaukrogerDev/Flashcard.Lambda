using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Flashcards.Application.Decks;
using Flashcards.Domain.Decks;

namespace Flashcards.Infrastructure.Decks;

public class DeckDynamoDbRepository : IDeckRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private readonly string _userIdIndexName;

    public DeckDynamoDbRepository(IAmazonDynamoDB dynamoDb, string tableName, string userIdIndexName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
        _userIdIndexName = userIdIndexName;
    }

    public async Task SaveAsync(Deck deck, CancellationToken cancellationToken = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new() { S = deck.Id.Value.ToString() },
            ["Name"] = new() { S = deck.Name },
            ["CreatedAt"] = new() { S = deck.CreatedAt.ToString("O") },
            ["UserId"] = new() { S = deck.UserId }
        };

        if (deck.Description is not null)
            item["Description"] = new AttributeValue { S = deck.Description };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request, cancellationToken);
    }

    public async Task<(IReadOnlyList<Deck> Decks, string? NextPaginationToken)> GetByUserIdAsync(
        string userId,
        int? pageSize = null,
        string? paginationToken = null,
        CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = _userIdIndexName,
            KeyConditionExpression = "UserId = :userId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":userId"] = new() { S = userId }
            }
        };

        if (pageSize.HasValue)
            request.Limit = pageSize.Value;

        if (paginationToken is not null)
            request.ExclusiveStartKey = DeserializePaginationToken(paginationToken);

        var response = await _dynamoDb.QueryAsync(request, cancellationToken);

        var decks = response.Items.Select(MapToDeck).ToList();
        var nextToken = response.LastEvaluatedKey?.Count > 0
            ? SerializePaginationToken(response.LastEvaluatedKey)
            : null;

        return (decks, nextToken);
    }

    private static Deck MapToDeck(Dictionary<string, AttributeValue> item)
        => Deck.Reconstitute(
            DeckId.From(Guid.Parse(item["Id"].S)),
            item["Name"].S,
            item.TryGetValue("Description", out var desc) ? desc.S : null,
            DateTime.Parse(item["CreatedAt"].S, null, System.Globalization.DateTimeStyles.RoundtripKind),
            item["UserId"].S);

    private static string SerializePaginationToken(Dictionary<string, AttributeValue> lastEvaluatedKey)
    {
        var serializable = lastEvaluatedKey.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.S);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(serializable)));
    }

    private static Dictionary<string, AttributeValue> DeserializePaginationToken(string token)
    {
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? throw new ArgumentException("Invalid pagination token.", nameof(token));
        return dict.ToDictionary(kvp => kvp.Key, kvp => new AttributeValue { S = kvp.Value });
    }
}
