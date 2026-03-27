using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Cards.AddCardToDeck;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;
using Flashcards.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class AddCardToDeckFunction
{
    private readonly AddCardToDeckCommandHandler _handler;

    public AddCardToDeckFunction() : this(BuildServiceProvider()) { }

    internal AddCardToDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<AddCardToDeckCommandHandler>();
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

            var body = JsonSerializer.Deserialize<AddCardRequestBody>(
                request.Body ?? string.Empty,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });

            if (body is null)
                return ErrorResponse(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new AddCardToDeckCommand(body.FrontText, body.BackText, deckId, userId, body.FrontPrompt, body.BackPrompt, body.BackgroundColour, body.TextColour);
            var response = await _handler.HandleAsync(command);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                })
            };
        }
        catch (DeckNotFoundException ex)
        {
            return ErrorResponse(HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorisedDeckAccessException)
        {
            return ErrorResponse(HttpStatusCode.Forbidden, "You do not have permission to add cards to this deck.");
        }
        catch (ArgumentException ex)
        {
            return ErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error adding card to deck: {ex}");
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

    private record AddCardRequestBody(
        string FrontText,
        string BackText,
        string? FrontPrompt = null,
        string? BackPrompt = null,
        CardColour? BackgroundColour = null,
        TextColour? TextColour = null);

    private static IServiceProvider BuildServiceProvider()
    {
        var deckTableName = Environment.GetEnvironmentVariable("DECK_TABLE_NAME")
            ?? throw new InvalidOperationException("Environment variable 'DECK_TABLE_NAME' is not set.");

        var deckUserIdIndexName = Environment.GetEnvironmentVariable("DECK_USER_ID_INDEX_NAME")
            ?? throw new InvalidOperationException("Environment variable 'DECK_USER_ID_INDEX_NAME' is not set.");

        var cardTableName = Environment.GetEnvironmentVariable("CARD_TABLE_NAME")
            ?? throw new InvalidOperationException("Environment variable 'CARD_TABLE_NAME' is not set.");

        var services = new ServiceCollection();
        services.AddInfrastructure(deckTableName, deckUserIdIndexName);
        services.AddCardInfrastructure(cardTableName);
        services.AddScoped<AddCardToDeckCommandHandler>();

        return services.BuildServiceProvider();
    }
}
