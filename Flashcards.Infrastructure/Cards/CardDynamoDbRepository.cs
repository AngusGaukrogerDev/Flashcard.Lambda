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
    private readonly PaginationTokenCodec? _paginationTokenCodec;

    public CardDynamoDbRepository(IAmazonDynamoDB dynamoDb, string tableName, string? deckIdIndexName = null)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
        _deckIdIndexName = deckIdIndexName;
        _paginationTokenCodec = deckIdIndexName is null
            ? null
            : new PaginationTokenCodec($"{tableName}:{deckIdIndexName}");
    }

    public async Task SaveAsync(Card card, CancellationToken cancellationToken = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new() { S = card.Id.Value.ToString() },
            ["FrontText"] = new() { S = card.FrontText },
            ["BackText"] = new() { S = card.BackText },
            ["DeckId"] = new() { S = card.DeckId },
            ["UserId"] = new() { S = card.UserId.Value },
            ["CreatedAt"] = new() { S = card.CreatedAt.ToString("O") }
        };

        if (card.NextReviewDate.HasValue)
            item["NextReviewDate"] = new AttributeValue { S = card.NextReviewDate.Value.ToString("O") };

        if (card.FrontPrompt is not null)
            item["FrontPrompt"] = new AttributeValue { S = card.FrontPrompt };

        if (card.BackPrompt is not null)
            item["BackPrompt"] = new AttributeValue { S = card.BackPrompt };

        if (card.BackgroundColour.HasValue)
            item["BackgroundColour"] = new AttributeValue { S = card.BackgroundColour.Value.ToString() };

        if (card.TextColour.HasValue)
            item["TextColour"] = new AttributeValue { S = card.TextColour.Value.ToString() };

        if (card.TagIds.Count > 0)
            item["TagIds"] = new AttributeValue { SS = card.TagIds.ToList() };

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

    public async Task<(IReadOnlyList<Card> Cards, string? NextPaginationToken)> GetByDeckIdAsync(
        string deckId,
        int? pageSize = null,
        string? paginationToken = null,
        CancellationToken cancellationToken = default)
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

        if (pageSize.HasValue)
            request.Limit = pageSize.Value;

        if (paginationToken is not null)
        {
            if (_paginationTokenCodec is null)
                throw new InvalidOperationException("Pagination token codec is not configured.");

            request.ExclusiveStartKey = _paginationTokenCodec.Deserialize(paginationToken);
        }

        var response = await _dynamoDb.QueryAsync(request, cancellationToken);

        var cards = response.Items.Select(MapToCard).ToList();
        var nextToken = response.LastEvaluatedKey?.Count > 0 && _paginationTokenCodec is not null
            ? _paginationTokenCodec.Serialize(response.LastEvaluatedKey)
            : null;

        return (cards, nextToken);
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

        string? frontPrompt = item.TryGetValue("FrontPrompt", out var fp) ? fp.S : null;
        string? backPrompt = item.TryGetValue("BackPrompt", out var bp) ? bp.S : null;

        CardColour? backgroundColour = item.TryGetValue("BackgroundColour", out var bc)
            ? Enum.Parse<CardColour>(bc.S)
            : null;

        TextColour? textColour = item.TryGetValue("TextColour", out var tc)
            ? Enum.Parse<TextColour>(tc.S)
            : null;

        IReadOnlyList<string>? tagIds = null;
        if (item.TryGetValue("TagIds", out var tagAttr) && tagAttr.SS is { Count: > 0 })
            tagIds = tagAttr.SS.OrderBy(x => x, StringComparer.Ordinal).ToList();

        return Card.Reconstitute(
            CardId.From(Guid.Parse(item["Id"].S)),
            item["FrontText"].S,
            item["BackText"].S,
            item["DeckId"].S,
            item["UserId"].S,
            DateTime.Parse(item["CreatedAt"].S, null, System.Globalization.DateTimeStyles.RoundtripKind),
            nextReviewDate,
            frontPrompt,
            backPrompt,
            backgroundColour,
            textColour,
            tagIds);
    }
}
