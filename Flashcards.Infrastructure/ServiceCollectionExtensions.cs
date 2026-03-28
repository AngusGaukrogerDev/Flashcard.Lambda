using Amazon.DynamoDBv2;
using Flashcards.Application.Cards;
using Flashcards.Application.Decks;
using Flashcards.Application.DeckTags;
using Flashcards.Infrastructure.Cards;
using Flashcards.Infrastructure.Decks;
using Flashcards.Infrastructure.DeckTags;
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

        services.AddScoped<DeckDynamoDbRepository>(sp =>
        {
            var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
            return new DeckDynamoDbRepository(dynamoDb, deckTableName, userIdIndexName);
        });
        services.AddScoped<IDeckReadRepository>(sp => sp.GetRequiredService<DeckDynamoDbRepository>());
        services.AddScoped<IDeckWriteRepository>(sp => sp.GetRequiredService<DeckDynamoDbRepository>());

        return services;
    }

    public static IServiceCollection AddDeckTagInfrastructure(
        this IServiceCollection services,
        string deckTagTableName,
        string deckTagDeckIdIndexName)
    {
        services.TryAddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());

        services.AddScoped<DeckTagDynamoDbRepository>(sp =>
        {
            var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
            return new DeckTagDynamoDbRepository(dynamoDb, deckTagTableName, deckTagDeckIdIndexName);
        });
        services.AddScoped<IDeckTagReadRepository>(sp => sp.GetRequiredService<DeckTagDynamoDbRepository>());
        services.AddScoped<IDeckTagWriteRepository>(sp => sp.GetRequiredService<DeckTagDynamoDbRepository>());

        return services;
    }

    public static IServiceCollection AddCardInfrastructure(
        this IServiceCollection services,
        string cardTableName,
        string? cardDeckIdIndexName = null)
    {
        services.TryAddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());

        services.AddScoped<CardDynamoDbRepository>(sp =>
        {
            var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
            return new CardDynamoDbRepository(dynamoDb, cardTableName, cardDeckIdIndexName);
        });
        services.AddScoped<ICardReadRepository>(sp => sp.GetRequiredService<CardDynamoDbRepository>());
        services.AddScoped<ICardWriteRepository>(sp => sp.GetRequiredService<CardDynamoDbRepository>());

        return services;
    }
}
