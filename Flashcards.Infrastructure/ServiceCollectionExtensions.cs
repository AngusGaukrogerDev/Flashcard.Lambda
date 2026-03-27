using Amazon.DynamoDBv2;
using Flashcards.Application.Decks;
using Flashcards.Infrastructure.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string deckTableName)
    {
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());

        services.AddScoped<IDeckRepository>(sp =>
        {
            var dynamoDb = sp.GetRequiredService<IAmazonDynamoDB>();
            return new DeckDynamoDbRepository(dynamoDb, deckTableName);
        });

        return services;
    }
}
