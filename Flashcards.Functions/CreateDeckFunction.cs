using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Flashcards.Functions;

public class CreateDeckFunction
{
    public APIGatewayHttpApiV2ProxyResponse FunctionHandler(
        APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var body = request.Body; // "Hello World"
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = System.Text.Json.JsonSerializer.Serialize(body?.ToUpper())
        };
    }
}