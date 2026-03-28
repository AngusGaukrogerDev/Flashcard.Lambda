using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Cards.GetCardById;
using Flashcards.Application.Cards.ReviewCard;
using Flashcards.Domain.Cards;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class ReviewCardFunction
{
    private readonly ICommandHandler<ReviewCardCommand, GetCardByIdResponse> _handler;

    public ReviewCardFunction() : this(FunctionServiceProviderFactory.BuildCardOnly(services =>
    {
        services.AddScoped<ReviewCardCommandHandler>();
        services.AddScoped<ICommandHandler<ReviewCardCommand, GetCardByIdResponse>>(sp => sp.GetRequiredService<ReviewCardCommandHandler>());
    })) { }

    internal ReviewCardFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<ReviewCardCommand, GetCardByIdResponse>>();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
        APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            var userId = LambdaRequestAuth.TryGetUserId(request);

            if (string.IsNullOrEmpty(userId))
                return ApiResponses.Error(HttpStatusCode.Unauthorized, "Unauthorised.");

            string? cardId = null;
            request.PathParameters?.TryGetValue("cardId", out cardId);

            if (string.IsNullOrEmpty(cardId))
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Card ID is required.");

            ReviewCardRequestBody? body;
            try
            {
                body = JsonSerializer.Deserialize<ReviewCardRequestBody>(
                    request.Body ?? string.Empty,
                    JsonDefaults.ReadOptions);
            }
            catch (JsonException)
            {
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Request body must include a valid rating (incorrect, hard, medium, easy).");
            }

            if (body is null)
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new ReviewCardCommand(cardId, userId, body.Rating);
            var response = await _handler.HandleAsync(command);

            return ApiResponses.Json(HttpStatusCode.OK, response);
        }
        catch (CardNotFoundException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Card not found.");
        }
        catch (UnauthorisedCardAccessException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Card not found.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error reviewing card: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private record ReviewCardRequestBody(RecallRating Rating);
}
