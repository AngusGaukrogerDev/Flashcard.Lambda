using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Cards.GetCardById;
using Flashcards.Domain.Cards;
using Flashcards.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetCardFunction
{
    private readonly GetCardByIdQueryHandler _handler;

    public GetCardFunction() : this(BuildServiceProvider()) { }

    internal GetCardFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<GetCardByIdQueryHandler>();
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

            string? cardId = null;
            request.PathParameters?.TryGetValue("cardId", out cardId);

            if (string.IsNullOrEmpty(cardId))
                return ErrorResponse(HttpStatusCode.BadRequest, "Card ID is required.");

            var query = new GetCardByIdQuery(cardId, userId);
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
        catch (CardNotFoundException ex)
        {
            return ErrorResponse(HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorisedCardAccessException)
        {
            return ErrorResponse(HttpStatusCode.Forbidden, "You do not have permission to access this card.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error retrieving card: {ex}");
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
        var cardTableName = Environment.GetEnvironmentVariable("CARD_TABLE_NAME")
            ?? throw new InvalidOperationException("Environment variable 'CARD_TABLE_NAME' is not set.");

        var services = new ServiceCollection();
        services.AddCardInfrastructure(cardTableName);
        services.AddScoped<GetCardByIdQueryHandler>();

        return services.BuildServiceProvider();
    }
}
