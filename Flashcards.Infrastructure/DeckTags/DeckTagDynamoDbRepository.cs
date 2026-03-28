using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Flashcards.Application.DeckTags;
using Flashcards.Domain.DeckTags;

namespace Flashcards.Infrastructure.DeckTags;

public class DeckTagDynamoDbRepository : IDeckTagRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private readonly string _deckIdIndexName;

    public DeckTagDynamoDbRepository(IAmazonDynamoDB dynamoDb, string tableName, string deckIdIndexName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
        _deckIdIndexName = deckIdIndexName;
    }

    public async Task SaveAsync(DeckTag tag, CancellationToken cancellationToken = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new() { S = tag.Id.Value.ToString() },
            ["DeckId"] = new() { S = tag.DeckId },
            ["Name"] = new() { S = tag.Name },
            ["UserId"] = new() { S = tag.UserId.Value },
            ["CreatedAt"] = new() { S = tag.CreatedAt.ToString("O") }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request, cancellationToken);
    }

    public async Task<DeckTag?> GetByIdAsync(string tagId, CancellationToken cancellationToken = default)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new() { S = tagId }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request, cancellationToken);

        return response.Item?.Count > 0 ? MapToDeckTag(response.Item) : null;
    }

    public async Task<IReadOnlyList<DeckTag>> GetByDeckIdAsync(string deckId, CancellationToken cancellationToken = default)
    {
        var result = new List<DeckTag>();
        Dictionary<string, AttributeValue>? exclusiveStartKey = null;

        do
        {
            var request = new QueryRequest
            {
                TableName = _tableName,
                IndexName = _deckIdIndexName,
                KeyConditionExpression = "DeckId = :deckId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":deckId"] = new() { S = deckId }
                },
                ExclusiveStartKey = exclusiveStartKey
            };

            var response = await _dynamoDb.QueryAsync(request, cancellationToken);
            result.AddRange(response.Items.Select(MapToDeckTag));

            exclusiveStartKey = response.LastEvaluatedKey?.Count > 0 ? response.LastEvaluatedKey : null;
        } while (exclusiveStartKey is not null);

        return result;
    }

    public async Task DeleteAsync(string tagId, CancellationToken cancellationToken = default)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new() { S = tagId }
            }
        };

        await _dynamoDb.DeleteItemAsync(request, cancellationToken);
    }

    public async Task DeleteAllForDeckAsync(string deckId, CancellationToken cancellationToken = default)
    {
        var tags = await GetByDeckIdAsync(deckId, cancellationToken);
        foreach (var tag in tags)
            await DeleteAsync(tag.Id.Value.ToString(), cancellationToken);
    }

    private static DeckTag MapToDeckTag(Dictionary<string, AttributeValue> item)
        => DeckTag.Reconstitute(
            DeckTagId.From(Guid.Parse(item["Id"].S)),
            item["DeckId"].S,
            item["Name"].S,
            item["UserId"].S,
            DateTime.Parse(item["CreatedAt"].S, null, System.Globalization.DateTimeStyles.RoundtripKind));
}
