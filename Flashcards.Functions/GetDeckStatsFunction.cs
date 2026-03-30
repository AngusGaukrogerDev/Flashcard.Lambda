using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Decks.GetDeckStats;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetDeckStatsFunction
{
    private readonly IQueryHandler<GetDeckStatsQuery, GetDeckStatsResponse> _handler;

    public GetDeckStatsFunction() : this(FunctionServiceProviderFactory.BuildDeckAndCardWithoutTags(services =>
    {
        services.AddScoped<GetDeckStatsQueryHandler>();
        services.AddScoped<IQueryHandler<GetDeckStatsQuery, GetDeckStatsResponse>>(sp => sp.GetRequiredService<GetDeckStatsQueryHandler>());
    })) { }

    internal GetDeckStatsFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<IQueryHandler<GetDeckStatsQuery, GetDeckStatsResponse>>();
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

            var query = new GetDeckStatsQuery(deckId, userId);
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
            context.Logger.LogError($"Unhandled error retrieving deck stats: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}

