using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Decks.CreateDeck;
using Flashcards.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class CreateDeckFunction
{
    private readonly CreateDeckCommandHandler _handler;

    public CreateDeckFunction() : this(BuildServiceProvider()) { }

    internal CreateDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<CreateDeckCommandHandler>();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
        APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            var command = JsonSerializer.Deserialize<CreateDeckCommand>(
                request.Body ?? string.Empty,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (command is null)
                return ErrorResponse(HttpStatusCode.BadRequest, "Request body is required.");

            var response = await _handler.HandleAsync(command);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = JsonSerializer.Serialize(response)
            };
        }
        catch (ArgumentException ex)
        {
            return ErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error creating deck: {ex}");
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

        var services = new ServiceCollection();
        services.AddInfrastructure(deckTableName);
        services.AddScoped<CreateDeckCommandHandler>();

        return services.BuildServiceProvider();
    }
}
