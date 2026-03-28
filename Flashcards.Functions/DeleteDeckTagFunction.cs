using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.DeckTags.DeleteDeckTag;
using Flashcards.Domain.DeckTags;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class DeleteDeckTagFunction
{
    private readonly ICommandHandler<DeleteDeckTagCommand> _handler;

    public DeleteDeckTagFunction() : this(FunctionServiceProviderFactory.BuildDeckCardAndTags(services =>
    {
        services.AddScoped<DeleteDeckTagCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteDeckTagCommand>>(sp => sp.GetRequiredService<DeleteDeckTagCommandHandler>());
    })) { }

    internal DeleteDeckTagFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<DeleteDeckTagCommand>>();
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

            string? tagId = null;
            request.PathParameters?.TryGetValue("tagId", out tagId);

            if (string.IsNullOrEmpty(deckId))
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Deck ID is required.");

            if (string.IsNullOrEmpty(tagId))
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Tag ID is required.");

            var command = new DeleteDeckTagCommand(deckId, tagId, userId);
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
        catch (DeckTagNotFoundException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Deck tag not found.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error deleting deck tag: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
