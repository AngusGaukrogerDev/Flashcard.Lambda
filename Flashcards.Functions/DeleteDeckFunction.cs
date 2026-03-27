using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Decks.DeleteDeck;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class DeleteDeckFunction
{
    private readonly ICommandHandler<DeleteDeckCommand> _handler;

    public DeleteDeckFunction() : this(FunctionServiceProviderFactory.BuildDeckOnly(services =>
    {
        services.AddScoped<DeleteDeckCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteDeckCommand>>(sp => sp.GetRequiredService<DeleteDeckCommandHandler>());
    })) { }

    internal DeleteDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<DeleteDeckCommand>>();
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

            var command = new DeleteDeckCommand(deckId, userId);
            await _handler.HandleAsync(command);

            return ApiResponses.NoContent();
        }
        catch (DeckNotFoundException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Deck not found.");
        }
        catch (UnauthorisedDeckAccessException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Deck not found.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error deleting deck: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
