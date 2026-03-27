using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace Flashcards.Functions;

internal static class ApiResponses
{
    private static readonly Dictionary<string, string> JsonHeaders = new() { { "Content-Type", "application/json" } };

    public static APIGatewayHttpApiV2ProxyResponse Json(HttpStatusCode statusCode, object body)
        => new()
        {
            StatusCode = (int)statusCode,
            Headers = JsonHeaders,
            Body = JsonSerializer.Serialize(body, JsonDefaults.WriteOptions)
        };

    public static APIGatewayHttpApiV2ProxyResponse NoContent()
        => new()
        {
            StatusCode = (int)HttpStatusCode.NoContent,
            Headers = JsonHeaders
        };

    public static APIGatewayHttpApiV2ProxyResponse Error(HttpStatusCode statusCode, string message)
        => new()
        {
            StatusCode = (int)statusCode,
            Headers = JsonHeaders,
            Body = JsonSerializer.Serialize(new { error = message }, JsonDefaults.WriteOptions)
        };
}
