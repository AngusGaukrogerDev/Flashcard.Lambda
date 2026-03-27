using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Cards.DeleteCard;
using Flashcards.Domain.Cards;
using Microsoft.Extensions.DependencyInjection;

namespace Flashcards.Functions;

public class DeleteCardFunction
{
    private readonly ICommandHandler<DeleteCardCommand> _handler;

    public DeleteCardFunction() : this(FunctionServiceProviderFactory.BuildCardOnly(services =>
    {
        services.AddScoped<DeleteCardCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteCardCommand>>(sp => sp.GetRequiredService<DeleteCardCommandHandler>());
    })) { }

    internal DeleteCardFunction(IServiceProvider serviceProvider)
    {
        _handler = serviceProvider.GetRequiredService<ICommandHandler<DeleteCardCommand>>();
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

            var command = new DeleteCardCommand(cardId, userId);
            await _handler.HandleAsync(command);

            return ApiResponses.NoContent();
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
            context.Logger.LogError($"Unhandled error deleting card: {ex}");
            return ApiResponses.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }
}
