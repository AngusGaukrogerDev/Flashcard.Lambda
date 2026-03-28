using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Cards.UpdateCard;
using Flashcards.Domain.Cards;
using Flashcards.Domain.DeckTags;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class UpdateCardFunction
{
    private readonly ICommandHandler<UpdateCardCommand, UpdateCardResponse> _handler;

    public UpdateCardFunction() : this(FunctionServiceProviderFactory.BuildCardAndDeckTags(services =>
    {
        services.AddScoped<UpdateCardCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCardCommand, UpdateCardResponse>>(sp => sp.GetRequiredService<UpdateCardCommandHandler>());
    })) { }

    internal UpdateCardFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<UpdateCardCommand, UpdateCardResponse>>();
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

            var body = JsonSerializer.Deserialize<UpdateCardRequestBody>(
                request.Body ?? string.Empty,
                JsonDefaults.ReadOptions);

            if (body is null)
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new UpdateCardCommand(cardId, userId, body.FrontText, body.BackText, body.FrontPrompt, body.BackPrompt, body.BackgroundColour, body.TextColour, body.TagIds);
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
        catch (InvalidDeckTagForDeckException ex)
        {
            return ApiResponses.Error(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return ApiResponses.Error(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error updating card: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private record UpdateCardRequestBody(
        string FrontText,
        string BackText,
        string? FrontPrompt = null,
        string? BackPrompt = null,
        CardColour? BackgroundColour = null,
        TextColour? TextColour = null,
        IReadOnlyList<string>? TagIds = null);

}
