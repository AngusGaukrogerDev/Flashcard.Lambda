using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Flashcards.Application.Decks;
using Flashcards.Domain.Decks;

namespace Flashcards.Infrastructure.Decks;

public class DeckDynamoDbRepository : IDeckRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DeckDynamoDbRepository(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public async Task SaveAsync(Deck deck, CancellationToken cancellationToken = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new() { S = deck.Id.Value.ToString() },
            ["Name"] = new() { S = deck.Name },
            ["CreatedAt"] = new() { S = deck.CreatedAt.ToString("O") }
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
}
