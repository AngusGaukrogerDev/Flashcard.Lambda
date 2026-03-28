using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.DeckTags.UpdateDeckTag;
using Flashcards.Domain.DeckTags;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class UpdateDeckTagFunction
{
    private readonly ICommandHandler<UpdateDeckTagCommand, UpdateDeckTagResponse> _handler;

    public UpdateDeckTagFunction() : this(FunctionServiceProviderFactory.BuildDeckWithTags(services =>
    {
        services.AddScoped<UpdateDeckTagCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateDeckTagCommand, UpdateDeckTagResponse>>(sp => sp.GetRequiredService<UpdateDeckTagCommandHandler>());
    })) { }

    internal UpdateDeckTagFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<UpdateDeckTagCommand, UpdateDeckTagResponse>>();
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

            var body = JsonSerializer.Deserialize<UpdateDeckTagRequestBody>(
                request.Body ?? string.Empty,
                JsonDefaults.ReadOptions);

            if (body is null)
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new UpdateDeckTagCommand(deckId, tagId, userId, body.Name);
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
        catch (DeckTagNotFoundException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Deck tag not found.");
        }
        catch (ArgumentException ex)
        {
            return ApiResponses.Error(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error updating deck tag: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private record UpdateDeckTagRequestBody(string Name);
}
