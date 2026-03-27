using Amazon.DynamoDBv2;
using Flashcards.Application.Cards;
using Flashcards.Application.Decks;
using Flashcards.Infrastructure.Cards;
using Flashcards.Infrastructure.Decks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Flashcards.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string deckTableName,
        string userIdIndexName)
    {
        services.TryAddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());

        services.AddScoped<IDeckRepository>(sp =>
        {
            var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
            return new DeckDynamoDbRepository(dynamoDb, deckTableName, userIdIndexName);
        });

        return services;
    }

    public static IServiceCollection AddCardInfrastructure(
        this IServiceCollection services,
        string cardTableName,
        string? cardDeckIdIndexName = null)
    {
        services.TryAddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());

        services.AddScoped<ICardRepository>(sp =>
        {
            var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
            return new CardDynamoDbRepository(dynamoDb, cardTableName, cardDeckIdIndexName);
        });

        return services;
    }
}
