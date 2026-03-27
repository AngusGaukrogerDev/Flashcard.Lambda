using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Decks.CreateDeck;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class CreateDeckFunction
{
    private readonly ICommandHandler<CreateDeckCommand, CreateDeckResponse> _handler;

    public CreateDeckFunction() : this(FunctionServiceProviderFactory.BuildDeckOnly(services =>
    {
        services.AddScoped<CreateDeckCommandHandler>();
        services.AddScoped<ICommandHandler<CreateDeckCommand, CreateDeckResponse>>(sp => sp.GetRequiredService<CreateDeckCommandHandler>());
    })) { }

    internal CreateDeckFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<CreateDeckCommand, CreateDeckResponse>>();
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

            var body = JsonSerializer.Deserialize<CreateDeckRequestBody>(
                request.Body ?? string.Empty,
                JsonDefaults.ReadOptions);

            if (body is null)
                return ApiResponses.Error(HttpStatusCode.BadRequest, "Request body is required.");

            var command = new CreateDeckCommand(body.Name, body.Description, userId);

            var response = await _handler.HandleAsync(command);

            return ApiResponses.Json(HttpStatusCode.Created, response);
        }
        catch (ArgumentException ex)
        {
            return ApiResponses.Error(HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled error creating deck: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private record CreateDeckRequestBody(string Name, string? Description);

}
