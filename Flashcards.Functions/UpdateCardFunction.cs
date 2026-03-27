using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Cards.UpdateCard;
using Flashcards.Domain.Cards;
using Flashcards.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class UpdateCardFunction
{
    private readonly UpdateCardCommandHandler _handler;

    public UpdateCardFunction() : this(BuildServiceProvider()) { }

    internal UpdateCardFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<UpdateCardCommandHandler>();
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

            var body = JsonSerializer.Deserialize<UpdateCardRequestBody>(
                request.Body ?? string.Empty,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });

            if (body is null)
                return ErrorResponse(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new UpdateCardCommand(cardId, userId, body.FrontText, body.BackText, body.FrontPrompt, body.BackPrompt, body.BackgroundColour, body.TextColour);
            var response = await _handler.HandleAsync(command);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                })
            };
        }
        catch (CardNotFoundException ex)
        {
            return ErrorResponse(HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorisedCardAccessException)
        {
            return ErrorResponse(HttpStatusCode.Forbidden, "You do not have permission to update this card.");
        }
        catch (ArgumentException ex)
        {
            return ErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error updating card: {ex}");
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

    private record UpdateCardRequestBody(
        string FrontText,
        string BackText,
        string? FrontPrompt = null,
        string? BackPrompt = null,
        CardColour? BackgroundColour = null,
        TextColour? TextColour = null);

    private static IServiceProvider BuildServiceProvider()
    {
        var cardTableName = Environment.GetEnvironmentVariable("CARD_TABLE_NAME")
            ?? throw new InvalidOperationException("Environment variable 'CARD_TABLE_NAME' is not set.");

        var services = new ServiceCollection();
        services.AddCardInfrastructure(cardTableName);
        services.AddScoped<UpdateCardCommandHandler>();

        return services.BuildServiceProvider();
    }
}
