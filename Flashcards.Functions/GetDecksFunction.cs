using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Queries;
using Flashcards.Application.Decks.GetDecks;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetDecksFunction
{
    private readonly IQueryHandler<GetDecksQuery, GetDecksResponse> _handler;

    public GetDecksFunction() : this(FunctionServiceProviderFactory.BuildDeckOnly(services =>
    {
        services.AddScoped<GetDecksQueryHandler>();
        services.AddScoped<IQueryHandler<GetDecksQuery, GetDecksResponse>>(sp => sp.GetRequiredService<GetDecksQueryHandler>());
    })) { }

    internal GetDecksFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<IQueryHandler<GetDecksQuery, GetDecksResponse>>();
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

            var queryParams = request.QueryStringParameters;

            string? pageSizeRaw = null;
            string? paginationToken = null;

            queryParams?.TryGetValue("pageSize", out pageSizeRaw);
            queryParams?.TryGetValue("paginationToken", out paginationToken);

            int? pageSize = pageSizeRaw is not null && int.TryParse(pageSizeRaw, out var parsed) && parsed > 0
                ? parsed
                : null;

            var query = new GetDecksQuery(userId, pageSize, paginationToken);
            var response = await _handler.HandleAsync(query);

            return ApiResponses.Json(HttpStatusCode.OK, response);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error retrieving decks: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
