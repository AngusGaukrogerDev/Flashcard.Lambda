using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.DeckTags.GetDeckTags;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetDeckTagsFunction
{
    private readonly IQueryHandler<GetDeckTagsQuery, GetDeckTagsResponse> _handler;

    public GetDeckTagsFunction() : this(FunctionServiceProviderFactory.BuildDeckWithTags(services =>
    {
        services.AddScoped<GetDeckTagsQueryHandler>();
        services.AddScoped<IQueryHandler<GetDeckTagsQuery, GetDeckTagsResponse>>(sp => sp.GetRequiredService<GetDeckTagsQueryHandler>());
    })) { }

    internal GetDeckTagsFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<IQueryHandler<GetDeckTagsQuery, GetDeckTagsResponse>>();
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

            var query = new GetDeckTagsQuery(deckId, userId);
            var response = await _handler.HandleAsync(query);

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
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error listing deck tags: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
