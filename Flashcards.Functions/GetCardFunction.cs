using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Cards.GetCardById;
using Flashcards.Domain.Cards;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetCardFunction
{
    private readonly IQueryHandler<GetCardByIdQuery, GetCardByIdResponse> _handler;

    public GetCardFunction() : this(FunctionServiceProviderFactory.BuildCardOnly(services =>
    {
        services.AddScoped<GetCardByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetCardByIdQuery, GetCardByIdResponse>>(sp => sp.GetRequiredService<GetCardByIdQueryHandler>());
    })) { }

    internal GetCardFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<IQueryHandler<GetCardByIdQuery, GetCardByIdResponse>>();
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

            string? cardId = null;
            request.PathParameters?.TryGetValue("cardId", out cardId);

            if (string.IsNullOrEmpty(cardId))
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Card ID is required.");

            var query = new GetCardByIdQuery(cardId, userId);
            var response = await _handler.HandleAsync(query);

            return ApiResponses.Json(HttpStatusCode.OK, response);
        }
        catch (CardNotFoundException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Card not found.");
        }
        catch (UnauthorisedCardAccessException)
        {
            return ApiResponses.Error(HttpStatusCode.NotFound, "Card not found.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error retrieving card: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
