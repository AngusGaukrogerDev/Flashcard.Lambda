using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.DeckTags.CreateDeckTag;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class CreateDeckTagFunction
{
    private readonly ICommandHandler<CreateDeckTagCommand, CreateDeckTagResponse> _handler;

    public CreateDeckTagFunction() : this(FunctionServiceProviderFactory.BuildDeckWithTags(services =>
    {
        services.AddScoped<CreateDeckTagCommandHandler>();
        services.AddScoped<ICommandHandler<CreateDeckTagCommand, CreateDeckTagResponse>>(sp => sp.GetRequiredService<CreateDeckTagCommandHandler>());
    })) { }

    internal CreateDeckTagFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<CreateDeckTagCommand, CreateDeckTagResponse>>();
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

            var body = JsonSerializer.Deserialize<CreateDeckTagRequestBody>(
                request.Body ?? string.Empty,
                JsonDefaults.ReadOptions);

            if (body is null)
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new CreateDeckTagCommand(deckId, userId, body.Name);
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
            context.Logger.LogError($"Unhandled error creating deck tag: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private record CreateDeckTagRequestBody(string Name);
}
