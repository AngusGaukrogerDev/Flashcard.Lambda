using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Cards.GetCardsByDeck;
using Flashcards.Domain.Decks;
using Flashcards.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetCardsByDeckFunction
{
    private readonly GetCardsByDeckQueryHandler _handler;

    public GetCardsByDeckFunction() : this(BuildServiceProvider()) { }

    internal GetCardsByDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<GetCardsByDeckQueryHandler>();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
        APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            var claims = request.RequestContext?.Authorizer?.Jwt?.Claims;
            var userId = claims is not null && claims.TryGetValue("sub", out var sub) ? sub : null;

            if (string.IsNullOrEmpty(userId))
                return ErrorResponse(HttpStatusCode.Unauthorized, "Unauthorised.");

            string? deckId = null;
            request.PathParameters?.TryGetValue("deckId", out deckId);

            if (string.IsNullOrEmpty(deckId))
                return ErrorResponse(HttpStatusCode.BadRequest, "Deck ID is required.");

            var query = new GetCardsByDeckQuery(deckId, userId);
            var response = await _handler.HandleAsync(query);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };
        }
        catch (DeckNotFoundException ex)
        {
            return ErrorResponse(HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorisedDeckAccessException)
        {
            return ErrorResponse(HttpStatusCode.Forbidden, "You do not have permission to access this deck.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error retrieving cards for deck: {ex}");
            return ErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static APIGatewayHttpApiV2ProxyResponse ErrorResponse(HttpStatusCode statusCode, string message)
        => new()
        {
            StatusCode = (int)statusCode,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = JsonSerializer.Serialize(new { error = message })
        };

    private static IServiceProvider BuildServiceProvider()
    {
        var deckTableName = Environment.GetEnvironmentVariable("DECK_TABLE_NAME")
            ?? throw new InvalidOperationException("Environment variable 'DECK_TABLE_NAME' is not set.");

        var deckUserIdIndexName = Environment.GetEnvironmentVariable("DECK_USER_ID_INDEX_NAME")
            ?? throw new InvalidOperationException("Environment variable 'DECK_USER_ID_INDEX_NAME' is not set.");

        var cardTableName = Environment.GetEnvironmentVariable("CARD_TABLE_NAME")
            ?? throw new InvalidOperationException("Environment variable 'CARD_TABLE_NAME' is not set.");

        var cardDeckIdIndexName = Environment.GetEnvironmentVariable("CARD_DECK_ID_INDEX_NAME")
            ?? throw new InvalidOperationException("Environment variable 'CARD_DECK_ID_INDEX_NAME' is not set.");

        var services = new ServiceCollection();
        services.AddInfrastructure(deckTableName, deckUserIdIndexName);
        services.AddCardInfrastructure(cardTableName, cardDeckIdIndexName);
        services.AddScoped<GetCardsByDeckQueryHandler>();

        return services.BuildServiceProvider();
    }
}
