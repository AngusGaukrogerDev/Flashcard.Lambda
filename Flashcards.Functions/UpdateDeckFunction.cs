using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Decks.UpdateDeck;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class UpdateDeckFunction
{
    private readonly ICommandHandler<UpdateDeckCommand, UpdateDeckResponse> _handler;

    public UpdateDeckFunction() : this(FunctionServiceProviderFactory.BuildDeckOnly(services =>
    {
        services.AddScoped<UpdateDeckCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateDeckCommand, UpdateDeckResponse>>(sp => sp.GetRequiredService<UpdateDeckCommandHandler>());
    })) { }

    internal UpdateDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<UpdateDeckCommand, UpdateDeckResponse>>();
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

            string? deckId = null;
            request.PathParameters?.TryGetValue("deckId", out deckId);

            if (string.IsNullOrEmpty(deckId))
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Deck ID is required.");

            var body = JsonSerializer.Deserialize<UpdateDeckRequestBody>(
                request.Body ?? string.Empty,
                JsonDefaults.ReadOptions);

            if (body is null)
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new UpdateDeckCommand(deckId, userId, body.Name, body.Description);
            var response = await _handler.HandleAsync(command);

            return ApiResponses.Json(HttpStatusCode.OK, response);
        }
        catch (DeckNotFoundException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Deck not found.");
        }
        catch (UnauthorisedDeckAccessException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Deck not found.");
        }
        catch (ArgumentException ex)
        {
            return ApiResponses.Error(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error updating deck: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private record UpdateDeckRequestBody(string Name, string? Description);

}
