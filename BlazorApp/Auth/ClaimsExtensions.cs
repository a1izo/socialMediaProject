using System.Security.Claims;

namespace BlazorApp.Auth;

public static class ClaimsExtensions
{
    public const string ModeratorPolicy = "IsModerator";

    public static bool IsLoggedIn(this ClaimsPrincipal user)
    {
        return user.Identity is not null && user.Identity.IsAuthenticated;
    }

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        if (!user.IsLoggedIn())
        {
            return null;
        }

        string? idAsString = user.Claims.SingleOrDefault(c => c.Type == "Id")?.Value;
        return int.TryParse(idAsString, out int id) ? id : null;
    }

    public static bool IsModerator(this ClaimsPrincipal user)
    {
        return user.HasClaim("IsModerator", "True");
    }
}
