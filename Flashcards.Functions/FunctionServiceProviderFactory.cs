using Flashcards.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

internal static class FunctionServiceProviderFactory
{
    public static IServiceProvider BuildDeckOnly(Action<IServiceCollection> configureHandlers)
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(GetRequiredEnv("DECK_TABLE_NAME"), GetRequiredEnv("DECK_USER_ID_INDEX_NAME"));
        configureHandlers(services);
        return services.BuildServiceProvider();
    }

    public static IServiceProvider BuildDeckWithTags(Action<IServiceCollection> configureHandlers)
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(GetRequiredEnv("DECK_TABLE_NAME"), GetRequiredEnv("DECK_USER_ID_INDEX_NAME"));
        services.AddDeckTagInfrastructure(GetRequiredEnv("DECK_TAG_TABLE_NAME"), GetRequiredEnv("DECK_TAG_DECK_ID_INDEX_NAME"));
        configureHandlers(services);
        return services.BuildServiceProvider();
    }

    public static IServiceProvider BuildCardOnly(Action<IServiceCollection> configureHandlers)
    {
        var services = new ServiceCollection();
        services.AddCardInfrastructure(GetRequiredEnv("CARD_TABLE_NAME"));
        configureHandlers(services);
        return services.BuildServiceProvider();
    }

    public static IServiceProvider BuildCardAndDeckTags(Action<IServiceCollection> configureHandlers)
    {
        var services = new ServiceCollection();
        services.AddDeckTagInfrastructure(GetRequiredEnv("DECK_TAG_TABLE_NAME"), GetRequiredEnv("DECK_TAG_DECK_ID_INDEX_NAME"));
        services.AddCardInfrastructure(GetRequiredEnv("CARD_TABLE_NAME"));
        configureHandlers(services);
        return services.BuildServiceProvider();
    }

    public static IServiceProvider BuildDeckAndCard(Action<IServiceCollection> configureHandlers, bool requireCardDeckIndex = true)
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(GetRequiredEnv("DECK_TABLE_NAME"), GetRequiredEnv("DECK_USER_ID_INDEX_NAME"));
        services.AddDeckTagInfrastructure(GetRequiredEnv("DECK_TAG_TABLE_NAME"), GetRequiredEnv("DECK_TAG_DECK_ID_INDEX_NAME"));
        var cardTableName = GetRequiredEnv("CARD_TABLE_NAME");
        var cardDeckIndex = requireCardDeckIndex ? GetRequiredEnv("CARD_DECK_ID_INDEX_NAME") : null;
        services.AddCardInfrastructure(cardTableName, cardDeckIndex);
        configureHandlers(services);
        return services.BuildServiceProvider();
    }

    public static IServiceProvider BuildDeckCardAndTags(Action<IServiceCollection> configureHandlers)
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(GetRequiredEnv("DECK_TABLE_NAME"), GetRequiredEnv("DECK_USER_ID_INDEX_NAME"));
        services.AddDeckTagInfrastructure(GetRequiredEnv("DECK_TAG_TABLE_NAME"), GetRequiredEnv("DECK_TAG_DECK_ID_INDEX_NAME"));
        services.AddCardInfrastructure(GetRequiredEnv("CARD_TABLE_NAME"), GetRequiredEnv("CARD_DECK_ID_INDEX_NAME"));
        configureHandlers(services);
        return services.BuildServiceProvider();
    }

    private static string GetRequiredEnv(string name)
        => Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"Environment variable '{name}' is not set.");
}
