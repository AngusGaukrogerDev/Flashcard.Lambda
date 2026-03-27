using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Decks.GetDecks;
using Flashcards.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class GetDecksFunction
{
    private readonly GetDecksQueryHandler _handler;

    public GetDecksFunction() : this(BuildServiceProvider()) { }

    internal GetDecksFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<GetDecksQueryHandler>();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
        APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            var claims = request.RequestContext?.Authorizer?.Jwt?.Claims;
            var userId = claims is not null && claims.TryGetValue("sub", out var sub) ? sub : null;

            if (string.IsNullOrEmpty(userId))
                return ErrorResponse(HttpStatusCode.Unauthorized, "Unauthorised.");

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

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error retrieving decks: {ex}");
            return ErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static APIGatewayHttpApiV2ProxyResponse ErrorResponse(HttpStatusCode statusCode, string message)
        => new()
        {
            StatusCode = (int)statusCode,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = JsonSerializer.Serialize(new { error = message })
        };

    private static IServiceProvider BuildServiceProvider()
    {
        var deckTableName = Environment.GetEnvironmentVariable("DECK_TABLE_NAME")
            ?? throw new InvalidOperationException("Environment variable 'DECK_TABLE_NAME' is not set.");

        var userIdIndexName = Environment.GetEnvironmentVariable("DECK_USER_ID_INDEX_NAME")
            ?? throw new InvalidOperationException("Environment variable 'DECK_USER_ID_INDEX_NAME' is not set.");

        var services = new ServiceCollection();
        services.AddInfrastructure(deckTableName, userIdIndexName);
        services.AddScoped<GetDecksQueryHandler>();

        return services.BuildServiceProvider();
    }
}
