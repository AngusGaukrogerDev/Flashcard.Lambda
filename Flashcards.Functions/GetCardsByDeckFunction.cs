using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Cards.GetCardsByDeck;
using Flashcards.Domain.Decks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetCardsByDeckFunction
{
    private readonly IQueryHandler<GetCardsByDeckQuery, GetCardsByDeckResponse> _handler;

    public GetCardsByDeckFunction() : this(FunctionServiceProviderFactory.BuildDeckAndCard(services =>
    {
        services.AddScoped<GetCardsByDeckQueryHandler>();
        services.AddScoped<IQueryHandler<GetCardsByDeckQuery, GetCardsByDeckResponse>>(sp => sp.GetRequiredService<GetCardsByDeckQueryHandler>());
    })) { }

    internal GetCardsByDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<IQueryHandler<GetCardsByDeckQuery, GetCardsByDeckResponse>>();
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
            string? pageSizeRaw = null;
            string? paginationToken = null;
            queryParams?.TryGetValue("pageSize", out pageSizeRaw);
            queryParams?.TryGetValue("paginationToken", out paginationToken);
            int? pageSize = pageSizeRaw is not null && int.TryParse(pageSizeRaw, out var parsed) && parsed > 0
                ? parsed
                : null;

            var query = new GetCardsByDeckQuery(deckId, userId, pageSize, paginationToken);
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
            context.Logger.LogError($"Unhandled error retrieving cards for deck: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
