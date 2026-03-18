using System.Security.Claims;

namespace TravelNest.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue("userId")
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.Parse(userId!);
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("role")
            ?? user.FindFirstValue(ClaimTypes.Role) ?? "";
    }
}
