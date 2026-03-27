using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Decks.DeleteDeck;
using Flashcards.Domain.Decks;
using Flashcards.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class DeleteDeckFunction
{
    private readonly DeleteDeckCommandHandler _handler;

    public DeleteDeckFunction() : this(BuildServiceProvider()) { }

    internal DeleteDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<DeleteDeckCommandHandler>();
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

            var command = new DeleteDeckCommand(deckId, userId);
            await _handler.HandleAsync(command);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (DeckNotFoundException ex)
        {
            return ErrorResponse(HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorisedDeckAccessException)
        {
            return ErrorResponse(HttpStatusCode.Forbidden, "You do not have permission to delete this deck.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error deleting deck: {ex}");
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

        var userIdIndexName = Environment.GetEnvironmentVariable("DECK_USER_ID_INDEX_NAME")
            ?? throw new InvalidOperationException("Environment variable 'DECK_USER_ID_INDEX_NAME' is not set.");

        var services = new ServiceCollection();
        services.AddInfrastructure(deckTableName, userIdIndexName);
        services.AddScoped<DeleteDeckCommandHandler>();

        return services.BuildServiceProvider();
    }
}
