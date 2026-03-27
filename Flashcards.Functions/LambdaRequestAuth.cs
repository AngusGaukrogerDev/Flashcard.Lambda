using Amazon.Lambda.APIGatewayEvents;

namespace Flashcards.Functions;

internal static class LambdaRequestAuth
{
    public static string? TryGetUserId(APIGatewayHttpApiV2ProxyRequest request)
    {
        var claims = request.RequestContext?.Authorizer?.Jwt?.Claims;
        return claims is not null && claims.TryGetValue("sub", out var sub) ? sub : null;
    }
}
