using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Flashcards.Application.Cards;
using Flashcards.Domain.Cards;

namespace Flashcards.Infrastructure.Cards;

public class CardDynamoDbRepository : ICardRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private readonly string? _deckIdIndexName;

    public CardDynamoDbRepository(IAmazonDynamoDB dynamoDb, string tableName, string? deckIdIndexName = null)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
        _deckIdIndexName = deckIdIndexName;
    }

    public async Task SaveAsync(Card card, CancellationToken cancellationToken = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new() { S = card.Id.Value.ToString() },
            ["FrontText"] = new() { S = card.FrontText },
            ["BackText"] = new() { S = card.BackText },
            ["DeckId"] = new() { S = card.DeckId },
            ["UserId"] = new() { S = card.UserId },
            ["CreatedAt"] = new() { S = card.CreatedAt.ToString("O") }
        };

        if (card.NextReviewDate.HasValue)
            item["NextReviewDate"] = new AttributeValue { S = card.NextReviewDate.Value.ToString("O") };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request, cancellationToken);
    }

    public async Task<Card?> GetByIdAsync(string cardId, CancellationToken cancellationToken = default)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new() { S = cardId }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request, cancellationToken);

        return response.Item?.Count > 0 ? MapToCard(response.Item) : null;
    }

    public async Task<IReadOnlyList<Card>> GetByDeckIdAsync(string deckId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_deckIdIndexName))
            throw new InvalidOperationException("DeckId index name is not configured.");

        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = _deckIdIndexName,
            KeyConditionExpression = "DeckId = :deckId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":deckId"] = new() { S = deckId }
            }
        };

        var response = await _dynamoDb.QueryAsync(request, cancellationToken);

        return response.Items.Select(MapToCard).ToList();
    }

    public async Task DeleteAsync(string cardId, CancellationToken cancellationToken = default)
    {
        var request = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new() { S = cardId }
            }
        };

        await _dynamoDb.DeleteItemAsync(request, cancellationToken);
    }

    private static Card MapToCard(Dictionary<string, AttributeValue> item)
    {
        DateTime? nextReviewDate = item.TryGetValue("NextReviewDate", out var nrd)
            ? DateTime.Parse(nrd.S, null, System.Globalization.DateTimeStyles.RoundtripKind)
            : null;

        return Card.Reconstitute(
            CardId.From(Guid.Parse(item["Id"].S)),
            item["FrontText"].S,
            item["BackText"].S,
            item["DeckId"].S,
            item["UserId"].S,
            DateTime.Parse(item["CreatedAt"].S, null, System.Globalization.DateTimeStyles.RoundtripKind),
            nextReviewDate);
    }
}
