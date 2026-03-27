using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Cards.AddCardToDeck;
using Flashcards.Domain.Cards;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class AddCardToDeckFunction
{
    private readonly ICommandHandler<AddCardToDeckCommand, AddCardToDeckResponse> _handler;

    public AddCardToDeckFunction() : this(FunctionServiceProviderFactory.BuildDeckAndCard(services =>
    {
        services.AddScoped<AddCardToDeckCommandHandler>();
        services.AddScoped<ICommandHandler<AddCardToDeckCommand, AddCardToDeckResponse>>(sp => sp.GetRequiredService<AddCardToDeckCommandHandler>());
    }, requireCardDeckIndex: false)) { }

    internal AddCardToDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<AddCardToDeckCommand, AddCardToDeckResponse>>();
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

            var body = JsonSerializer.Deserialize<AddCardRequestBody>(
                request.Body ?? string.Empty,
                JsonDefaults.ReadOptions);

            if (body is null)
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new AddCardToDeckCommand(body.FrontText, body.BackText, deckId, userId, body.FrontPrompt, body.BackPrompt, body.BackgroundColour, body.TextColour);
            var response = await _handler.HandleAsync(command);

            return ApiResponses.Json(HttpStatusCode.Created, response);
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
            context.Logger.LogError($"Unhandled error adding card to deck: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private record AddCardRequestBody(
        string FrontText,
        string BackText,
        string? FrontPrompt = null,
        string? BackPrompt = null,
        CardColour? BackgroundColour = null,
        TextColour? TextColour = null);

}
