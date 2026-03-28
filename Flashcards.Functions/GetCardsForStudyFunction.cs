using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Cards.GetCardsForStudy;
using Flashcards.Application.Cards;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetCardsForStudyFunction
{
    private readonly IQueryHandler<GetCardsForStudyQuery, GetCardsForStudyResponse> _handler;

    public GetCardsForStudyFunction() : this(FunctionServiceProviderFactory.BuildDeckAndCard(services =>
    {
        services.AddScoped<GetCardsForStudyQueryHandler>();
        services.AddScoped<IQueryHandler<GetCardsForStudyQuery, GetCardsForStudyResponse>>(sp => sp.GetRequiredService<GetCardsForStudyQueryHandler>());
    })) { }

    internal GetCardsForStudyFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<IQueryHandler<GetCardsForStudyQuery, GetCardsForStudyResponse>>();
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

            var queryParams = request.QueryStringParameters;
            string? limitRaw = null;
            queryParams?.TryGetValue("limit", out limitRaw);
            var limit = limitRaw is not null && int.TryParse(limitRaw, out var parsed) && parsed > 0
                ? Math.Min(parsed, StudyQueueOrdering.MaxLimit)
                : 20;

            var query = new GetCardsForStudyQuery(deckId, userId, limit);
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
            context.Logger.LogError($"Unhandled error retrieving study cards: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
